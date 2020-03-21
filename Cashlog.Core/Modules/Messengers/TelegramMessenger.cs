using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Cashlog.Core.Common;
using Cashlog.Core.Models;
using Cashlog.Core.Models.Main;
using Cashlog.Core.Modules.Messengers.Menu;
using Cashlog.Core.Providers.Abstract;
using Cashlog.Core.Services.Abstract;
using Cashlog.Core.Workers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using File = Telegram.Bot.Types.File;
using MessageType = Telegram.Bot.Types.Enums.MessageType;

namespace Cashlog.Core.Modules.Messengers
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>В данный момент тут основная бизнес-логика, в последующем её необходимо вынести
    /// в отдельный класс, чтобы можно было независимо менять мессенджеры.</remarks>
    public class TelegramMessenger : IMessenger, IProxyConsumer
    {
        private readonly ICashlogSettingsService _cashlogSettingsService;
        private readonly IReceiptHandleService _receiptHandleService;
        private readonly IQueryDataSerializer _queryDataSerializer;
        private readonly ICustomerService _customerService;
        private readonly IGroupService _groupService;
        private readonly ILogger _logger;

        private TelegramBotClient _client;

        public TelegramMessenger(
            ICashlogSettingsService cashlogSettingsService,
            IReceiptHandleService receiptHandleService,
            IQueryDataSerializer queryDataSerializer,
            ICustomerService customerService,
            IGroupService groupService,
            ILogger logger)
        {
            _cashlogSettingsService = cashlogSettingsService ?? throw new ArgumentNullException(nameof(cashlogSettingsService));
            _receiptHandleService = receiptHandleService ?? throw new ArgumentNullException(nameof(receiptHandleService));
            _queryDataSerializer = queryDataSerializer ?? throw new ArgumentNullException(nameof(queryDataSerializer));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async void OnCallbackQuery(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            try
            {
                IQueryData queryData = _queryDataSerializer.DecodeBase64(e.CallbackQuery.Data);
                Group group = await _groupService.GetByChatTokenAsync(queryData.ChatToken);
                Customer[] customers = await _customerService.GetByGroupIdAsync(group.Id);

                // Собираем всю информацию о пользователе и сообщении.
                UserMessageInfo userMessageInfo = new UserMessageInfo
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

        public async Task EditMessageAsync(UserMessageInfo userMessageInfo, string text, IMenu menu = null)
        {
            await _client.EditMessageTextAsync(userMessageInfo.Group.ChatToken, int.Parse(userMessageInfo.Message.Token), text, replyMarkup: (InlineKeyboardMarkup) ((TelegramMenu) menu)?.Markup);
        }

        private async void OnMessageReceived(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            try
            {
                if (e.Message.Chat.Type != ChatType.Group)
                    return;

                long chatId = e.Message.Chat.Id;

                var settings = _cashlogSettingsService.ReadSettings();
                if (chatId.ToString() != settings.AdminChatToken)
                {
                    _logger.Info($"{GetType().Name}: Произведена попытка использования бота в группе `{e.Message.Chat.Title}` ({chatId})");
                    await _client.SendTextMessageAsync(chatId, "Чтобы бот мог работать в этой группе обратитесь к @vsoff");
                    return;
                }

                // Получаем группу этого чата.
                Group group = await _groupService.GetByChatTokenAsync(chatId.ToString());

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

                    group = await _groupService.AddAsync(e.Message.Chat.Id.ToString(), creator.User.Id.ToString(), e.Message.Chat.Title ?? "Default chat name");
                }

                Customer[] customers = await _customerService.GetByGroupIdAsync(group.Id);

                // Собираем всю информацию о пользователе и сообщении.
                UserMessageInfo userMessageInfo = new UserMessageInfo
                {
                    Group = group,
                    Customers = customers,
                    UserName = e.Message.From.Username,
                    UserToken = e.Message.From.Id.ToString(),
                    MessageType = Core.Models.MessageType.Unknown,
                    Message = new MessageInfo
                    {
                        //e.Message.ReplyToMessage.MessageId
                        Token = e.Message.MessageId.ToString(),
                        ReceiptInfo = null,
                        Text = e.Message.Text,
                    }
                };

                // Дописываем дополнительную инфу.
                switch (e.Message.Type)
                {
                    case MessageType.Photo:
                    {
                        PhotoSize photoSize = e.Message.Photo.OrderByDescending(x => x.Width).First();
                        _logger.Trace($"{GetType().Name}: Получено фото чека с разрешением W:{photoSize.Width} H:{photoSize.Height}");

                        File file = await _client.GetFileAsync(photoSize.FileId);

                        // Скачиваем изображение.
                        byte[] imageBytes;
                        try
                        {
                            await using MemoryStream clientStream = new MemoryStream();
                            await _client.DownloadFileAsync(file.FilePath, clientStream);
                            imageBytes = clientStream.ToArray();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Ошибка во время скачки изображения с чеком из telegram", ex);
                        }

                        _logger.Trace($"{GetType().Name}: Получено изображение из потока размером {imageBytes.Length} байт");

                        var data = _receiptHandleService.ParsePhoto(imageBytes);
                        _logger.Trace(data == null ? $"{GetType().Name}: Не удалось распознать QR код на чеке" : $"{GetType().Name}: Данные с QR кода чека {data.RawData}");

                        userMessageInfo.Message.ReceiptInfo = data;
                        userMessageInfo.MessageType = Core.Models.MessageType.QrCode;

                        break;
                    }

                    case MessageType.Text:
                    {
                        userMessageInfo.MessageType = Core.Models.MessageType.Text;
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

        public event EventHandler<UserMessageInfo> OnMessage;

        public async Task SendMessageAsync(UserMessageInfo userMessageInfo, string text, bool isReply = false, IMenu menu = null)
        {
            var replyToMessageId = isReply ? int.Parse(userMessageInfo.Message.Token) : 0;

            if (menu != null && menu.GetType() != typeof(TelegramMenu))
                throw new ArgumentException($"{GetType().Name} может работать только с {nameof(IMenu)} типа {nameof(TelegramMenu)}");

            var telegramMenu = menu as TelegramMenu;
            await _client.SendTextMessageAsync(userMessageInfo.Group.ChatToken, text, replyToMessageId: replyToMessageId, replyMarkup: telegramMenu?.Markup);
        }

        public void OnProxyChanged(WebProxy proxy)
        {
            // Удаляем старый клиент.
            var oldClient = _client;
            if (oldClient != null)
            {
                oldClient.StopReceiving();
                oldClient.OnMessage -= OnMessageReceived;
                oldClient.OnCallbackQuery -= OnCallbackQuery;
            }

            if (proxy == null)
            {
                _logger.Info($"{GetType().Name}: Бот выключен, так как не был получен новый прокси-сервер");
                return;
            }

            // Создаём новый клиент.
            var settings = _cashlogSettingsService.ReadSettings();
            var newClient = new TelegramBotClient(settings.TelegramBotToken, proxy);
            newClient.OnMessage += OnMessageReceived;
            newClient.OnCallbackQuery += OnCallbackQuery;
            newClient.StartReceiving();
            newClient.SendTextMessageAsync(settings.AdminChatToken, "Бот запущен под новым прокси-сервером!").GetAwaiter().GetResult();
            _client = newClient;

            _logger.Info($"{GetType().Name}: Telegram бот начал работать на прокси-сервере `{proxy.Address}`");
        }
    }
}