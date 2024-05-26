using Cashlog.Core.Common;
using Cashlog.Core.Models;
using Cashlog.Core.Modules.Messengers.Menu;
using Cashlog.Core.Options;
using Cashlog.Core.Services.Abstract;
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
    private readonly ILogger _logger;
    private readonly IQueryDataSerializer _queryDataSerializer;
    private readonly IReceiptHandleService _receiptHandleService;

    private TelegramBotClient _client;

    public TelegramMessenger(
        IOptions<CashlogOptions> cashlogOptions,
        IReceiptHandleService receiptHandleService,
        IQueryDataSerializer queryDataSerializer,
        ICustomerService customerService,
        IGroupService groupService,
        ILogger logger)
    {
        _cashlogOptions = cashlogOptions;
        _receiptHandleService = receiptHandleService;
        _queryDataSerializer = queryDataSerializer;
        _customerService = customerService;
        _groupService = groupService;
        _logger = logger;
    }

    public event EventHandler<UserMessageInfo> OnMessage;

    public async Task EditMessageAsync(UserMessageInfo userMessageInfo, string text, IMenu menu = null)
    {
        await _client.EditMessageTextAsync(userMessageInfo.Group.ChatToken, int.Parse(userMessageInfo.Message.Token),
            text, replyMarkup: (InlineKeyboardMarkup)((TelegramMenu)menu)?.Markup);
    }

    public void StartReceiving()
    {
        if (_client != null)
            throw new InvalidOperationException("Бот уже запущен");

        var cashlogOptions = _cashlogOptions.Value;
        if (string.IsNullOrEmpty(cashlogOptions.TelegramBotToken))
            throw new InvalidOperationException("Telegram токен не должен быть пустым");

        _client = new TelegramBotClient(cashlogOptions.TelegramBotToken);
        _client.OnMessage += OnMessageReceived;
        _client.OnCallbackQuery += OnCallbackQuery;

        _client.StartReceiving();

        _client.SendTextMessageAsync(cashlogOptions.AdminChatToken, "Бот запущен!").GetAwaiter().GetResult();
        _logger.Info($"{GetType().Name}: Telegram бот начал работать");
    }

    public void StopReceiving()
    {
        _client.StopReceiving();
    }

    public async Task SendMessageAsync(UserMessageInfo userMessageInfo, string text, bool isReply = false,
        IMenu menu = null)
    {
        var replyToMessageId = isReply ? int.Parse(userMessageInfo.Message.Token) : 0;

        if (menu != null && menu.GetType() != typeof(TelegramMenu))
            throw new ArgumentException(
                $"{GetType().Name} может работать только с {nameof(IMenu)} типа {nameof(TelegramMenu)}");

        var telegramMenu = menu as TelegramMenu;
        await _client.SendTextMessageAsync(userMessageInfo.Group.ChatToken, text, replyToMessageId: replyToMessageId,
            replyMarkup: telegramMenu?.Markup);
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
            _logger.Error($"{GetType().Name}: Произошла ошибка во время обработки query запроса ", ex);
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
                _logger.Info($"{GetType().Name}: Произведена попытка использования бота в группе `{e.Message.Chat.Title}` ({chatId})");
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
                    _logger.Warning($"В группе {chatId} не был найден создатель");
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
                    _logger.Trace(
                        $"{GetType().Name}: Получено фото чека с разрешением W:{photoSize.Width} H:{photoSize.Height}");

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

                    _logger.Trace(
                        $"{GetType().Name}: Получено изображение из потока размером {imageBytes.Length} байт");

                    var data = _receiptHandleService.ParsePhoto(imageBytes);
                    _logger.Trace(data == null
                        ? $"{GetType().Name}: Не удалось распознать QR код на чеке"
                        : $"{GetType().Name}: Данные с QR кода чека {data.RawData}");

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
            _logger.Error($"{GetType().Name}: Во время обработки полученного сообщения произошла ошибка", ex);
        }
    }
}