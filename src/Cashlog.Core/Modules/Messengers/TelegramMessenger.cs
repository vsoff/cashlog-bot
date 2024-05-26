using Cashlog.Core.Models;
using Cashlog.Core.Modules.Messengers.Menu;
using Cashlog.Core.Options;
using Cashlog.Core.Services.Abstract;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using MessageType = Telegram.Bot.Types.Enums.MessageType;

namespace Cashlog.Core.Modules.Messengers;

/// <summary>
/// </summary>
/// <remarks>
///     В данный момент тут основная бизнес-логика, в последующем её необходимо вынести
///     в отдельный класс, чтобы можно было независимо менять мессенджеры.
/// </remarks>
public class TelegramMessenger : IMessenger
{
    private readonly IOptions<CashlogOptions> _cashlogOptions;
    private readonly ICustomerService _customerService;
    private readonly IGroupService _groupService;
    private readonly ILogger<TelegramMessenger> _logger;
    private readonly IQueryDataSerializer _queryDataSerializer;
    private readonly IReceiptHandleService _receiptHandleService;

    private TelegramBotClient _client;

    public TelegramMessenger(
        IOptions<CashlogOptions> cashlogOptions,
        IReceiptHandleService receiptHandleService,
        IQueryDataSerializer queryDataSerializer,
        ICustomerService customerService,
        IGroupService groupService,
        ILogger<TelegramMessenger> logger)
    {
        _cashlogOptions = cashlogOptions;
        _receiptHandleService = receiptHandleService;
        _queryDataSerializer = queryDataSerializer;
        _customerService = customerService;
        _groupService = groupService;
        _logger = logger;
    }

    public event EventHandler<UserMessageInfo> OnMessage;

    public async ValueTask EditMessageAsync(UserMessageInfo userMessageInfo, string text, IMenu menu = null)
    {
        await _client.EditMessageTextAsync(userMessageInfo.Group.ChatToken, int.Parse(userMessageInfo.Message.Token),
            text, replyMarkup: (InlineKeyboardMarkup)((TelegramMenu)menu)?.Markup);
    }

    public async ValueTask StartReceivingAsync(CancellationToken cancellationToken)
    {
        if (_client != null)
            throw new InvalidOperationException("Бот уже запущен");

        var cashlogOptions = _cashlogOptions.Value;
        if (string.IsNullOrEmpty(cashlogOptions.TelegramBotToken))
            throw new InvalidOperationException("Telegram токен не должен быть пустым");

        _client = new TelegramBotClient(cashlogOptions.TelegramBotToken);
        _client.OnMessage += OnMessageReceived;
        _client.OnCallbackQuery += OnCallbackQuery;

        _client.StartReceiving(cancellationToken: cancellationToken);

        await _client.SendTextMessageAsync(
            cashlogOptions.AdminChatToken,
            text: "Бот запущен!",
            cancellationToken: cancellationToken);

        _logger.LogInformation("Telegram бот запущен");
    }

    public ValueTask StopReceivingAsync(CancellationToken cancellationToken)
    {
        _client.StopReceiving();
        _logger.LogInformation("Telegram бот остановлен");
        return ValueTask.CompletedTask;
    }

    public async ValueTask SendMessageAsync(UserMessageInfo userMessageInfo, string text, bool isReply = false,
        IMenu menu = null)
    {
        var replyToMessageId = isReply ? int.Parse(userMessageInfo.Message.Token) : 0;

        if (menu is not null && menu is not TelegramMenu)
            throw new ArgumentException(
                $"{nameof(TelegramMessenger)} может работать только с {nameof(IMenu)} типа {nameof(TelegramMenu)}");

        await _client.SendTextMessageAsync(userMessageInfo.Group.ChatToken, text, replyToMessageId: replyToMessageId,
            replyMarkup: (menu as TelegramMenu)?.Markup);
    }

    private async void OnCallbackQuery(object sender, CallbackQueryEventArgs e)
    {
        try
        {
            var queryData = _queryDataSerializer.DecodeBase64(e.CallbackQuery.Data);
            var group = await _groupService.GetByChatTokenAsync(queryData.ChatToken);
            var customers = await _customerService.GetListAsync(group.Id);

            // Собираем всю информацию о пользователе и сообщении.
            var userMessageInfo = new UserMessageInfo
            {
                Group = group,
                Customers = customers,
                UserName = e.CallbackQuery.From.Username,
                UserToken = e.CallbackQuery.From.Id.ToString(),
                MessageType = Models.MessageType.Query,
                Message = new MessageInfo
                {
                    Token = e.CallbackQuery.Message.MessageId.ToString(),
                    Text = e.CallbackQuery.Message.Text,
                    QueryData = queryData
                }
            };

            OnMessage?.Invoke(this, userMessageInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Произошла ошибка во время обработки query запроса ");
        }
    }

    private async void OnMessageReceived(object sender, MessageEventArgs e)
    {
        try
        {
            if (e.Message.Chat.Type != ChatType.Group)
                return;

            var chatId = e.Message.Chat.Id;

            if (chatId.ToString() != _cashlogOptions.Value.AdminChatToken)
            {
                _logger.LogInformation("Произведена попытка использования бота в группе `{Title}` ({ChatId})",
                    e.Message.Chat.Title,
                    chatId);

                await _client.SendTextMessageAsync(chatId, "Чтобы бот мог работать в этой группе обратитесь к @vsoff");
                return;
            }

            // Получаем группу этого чата.
            var group = await _groupService.GetByChatTokenAsync(chatId.ToString());

            // Если группы для этого чата нету, то создаём её.
            if (group == null)
            {
                var admins = await _client.GetChatAdministratorsAsync(chatId);
                var creator = admins.FirstOrDefault(x => x.Status == ChatMemberStatus.Creator);

                if (creator == null)
                {
                    _logger.LogWarning("В группе {ChatId} не был найден создатель", chatId);
                    return;
                }

                group = await _groupService.AddAsync(e.Message.Chat.Id.ToString(), creator.User.Id.ToString(),
                    e.Message.Chat.Title ?? "Default chat name");
            }

            var customers = await _customerService.GetListAsync(group.Id);

            // Собираем всю информацию о пользователе и сообщении.
            var userMessageInfo = new UserMessageInfo
            {
                Group = group,
                Customers = customers,
                UserName = e.Message.From.Username,
                UserToken = e.Message.From.Id.ToString(),
                MessageType = Models.MessageType.Unknown,
                Message = new MessageInfo
                {
                    //e.Message.ReplyToMessage.MessageId
                    Token = e.Message.MessageId.ToString(),
                    ReceiptInfo = null,
                    Text = e.Message.Text
                }
            };

            // Дописываем дополнительную инфу.
            switch (e.Message.Type)
            {
                case MessageType.Photo:
                {
                    var photoSize = e.Message.Photo.OrderByDescending(x => x.Width).First();
                    _logger.LogTrace(
                        "Получено фото чека с разрешением W:{Width} H:{Height}",
                        photoSize.Width,
                        photoSize.Height);

                    var file = await _client.GetFileAsync(photoSize.FileId);

                    // Скачиваем изображение.
                    byte[] imageBytes;
                    try
                    {
                        await using var clientStream = new MemoryStream();
                        await _client.DownloadFileAsync(file.FilePath, clientStream);
                        imageBytes = clientStream.ToArray();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Ошибка во время скачки изображения с чеком из telegram", ex);
                    }

                    _logger.LogTrace(
                        "Получено изображение из потока размером {Bytes} байт",
                        imageBytes.Length);

                    var data = _receiptHandleService.ParsePhoto(imageBytes);
                    _logger.LogTrace(data == null
                        ? "Не удалось распознать QR код на чеке"
                        : $"Данные с QR кода чека {data.RawData}");

                    userMessageInfo.Message.ReceiptInfo = data;
                    userMessageInfo.MessageType = Models.MessageType.QrCode;

                    break;
                }

                case MessageType.Text:
                {
                    userMessageInfo.MessageType = Models.MessageType.Text;
                    break;
                }
            }

            // Уведомляем подписчиков о новом сообщении.
            OnMessage?.Invoke(this, userMessageInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Во время обработки полученного сообщения произошла ошибка");
        }
    }
}