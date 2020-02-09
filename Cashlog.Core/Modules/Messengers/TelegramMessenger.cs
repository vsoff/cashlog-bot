using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Cashlog.Core.Common;
using Cashlog.Core.Core;
using Cashlog.Core.Messengers.Menu;
using Cashlog.Core.Core.Models;
using Cashlog.Core.Core.Services;
using Cashlog.Core.Core.Services.Abstract;
using Newtonsoft.Json;
using ProtoBuf;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using File = Telegram.Bot.Types.File;
using MessageType = Telegram.Bot.Types.Enums.MessageType;

namespace Cashlog.Core.Messengers
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>В данный момент тут основная бизнес-логика, в последующем её необходимо вынести
    /// в отдельный класс, чтобы можно было независимо менять мессенджеры.</remarks>
    public class TelegramMessenger : IMessenger
    {
        private readonly IMoneyOperationService _moneyOperationService;
        private readonly IReceiptHandleService _receiptHandleService;
        private readonly IBillingPeriodService _billingPeriodService;
        private readonly IQueryDataSerializer _queryDataSerializer;
        private readonly ICustomerService _customerService;
        private readonly IReceiptService _receiptService;
        private readonly CashlogSettings _cashogSettings;
        private readonly IGroupService _groupService;
        private readonly IMenuProvider _menuProvider;
        private readonly ILogger _logger;
        private readonly TelegramBotClient _client;

        public TelegramMessenger(
            IMoneyOperationService moneyOperationService,
            IReceiptHandleService receiptHandleService,
            IBillingPeriodService billingPeriodService,
            IQueryDataSerializer queryDataSerializer,
            ICustomerService customerService,
            IReceiptService receiptService,
            CashlogSettings cashogSettings,
            IGroupService groupService,
            IMenuProvider menuProvider,
            ILogger logger)
        {
            _moneyOperationService = moneyOperationService ?? throw new ArgumentNullException(nameof(moneyOperationService));
            _receiptHandleService = receiptHandleService ?? throw new ArgumentNullException(nameof(receiptHandleService));
            _billingPeriodService = billingPeriodService ?? throw new ArgumentNullException(nameof(billingPeriodService));
            _queryDataSerializer = queryDataSerializer ?? throw new ArgumentNullException(nameof(queryDataSerializer));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _receiptService = receiptService ?? throw new ArgumentNullException(nameof(receiptService));
            _cashogSettings = cashogSettings ?? throw new ArgumentNullException(nameof(cashogSettings));
            _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
            _menuProvider = menuProvider ?? throw new ArgumentNullException(nameof(menuProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            IWebProxy proxy = new WebProxy(_cashogSettings.ProxyAddress);
            _client = new TelegramBotClient(_cashogSettings.TelegramBotToken, proxy);

            // TODO Убрать и сделать проверку постоянной.
            _logger.Info("Проверка прокси...");
            var checkResult = _client.GetMeAsync().Result;
            _client.SendTextMessageAsync(_cashogSettings.AdminChatToken, "helllo");
            _logger.Info("Прокси работает!");

            _client.OnMessage += OnMessageReceived;
            _client.OnCallbackQuery += OnCallbackQuery;
            _client.StartReceiving();
        }

        private async void OnCallbackQuery(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
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
                MessageType = Core.Models.MessageType.Command,
                Message = new MessageInfo
                {
                    Token = e.CallbackQuery.Message.MessageId.ToString(),
                    Text = e.CallbackQuery.Message.Text
                }
            };

            // TODO Вынести отсюда логику обработки в MessagesHandler.
            // Todo Проверка №3. Надо перенести в одно место.
            switch (queryData.MenuType)
            {
                case MenuType.NewReceiptSelectCustomer:
                {
                    var data = (AddReceiptQueryData) queryData;
                    if (data.TargetId == data.SelectedCustomerId)
                        break;

                    data.SelectedCustomerId = data.TargetId;

                    IMenu menu = _menuProvider.GetMenu(userMessageInfo, data);
                    await EditMessageAsync(userMessageInfo, "Кто оплатил чек?", menu);
                    break;
                }
                case MenuType.NewReceiptSelectConsumers:
                {
                    var data = (AddReceiptQueryData) queryData;
                    var selectedConsumers = new List<long>(data.SelectedConsumerIds ?? new long[0]);
                    if (data.TargetId != null)
                    {
                        if (selectedConsumers.Contains(data.TargetId.Value))
                            selectedConsumers.Remove(data.TargetId.Value);
                        else
                            selectedConsumers.Add(data.TargetId.Value);
                    }

                    selectedConsumers = selectedConsumers.Distinct().ToList();
                    data.SelectedConsumerIds = selectedConsumers.ToArray();

                    IMenu menu = _menuProvider.GetMenu(userMessageInfo, data);
                    await EditMessageAsync(userMessageInfo, "На кого делится чек?", menu);
                    break;
                }
                case MenuType.NewReceiptAdd:
                {
                    var data = (AddReceiptQueryData) queryData;
                    string customerText = userMessageInfo.Customers.FirstOrDefault(x => data.SelectedCustomerId.Value == x.Id).Caption;
                    string[] consumerTexts = userMessageInfo.Customers
                        .Where(x => data.SelectedCustomerId.Value == x.Id || data.SelectedConsumerIds.Contains(x.Id))
                        .Select(x => x.Caption).ToArray();
                    Receipt receipt = await _receiptService.GetAsync(data.ReceiptId);
                    await EditMessageAsync(userMessageInfo, $"Чек готов!\nОплатил: {customerText}\nСумма: {receipt.TotalAmount} руб.\nДелится на: {string.Join(", ", consumerTexts)}");
                    if (receipt.Status == ReceiptStatus.New || receipt.Status == ReceiptStatus.NewManual)
                    {
                        receipt.Status = receipt.Status == ReceiptStatus.New ? ReceiptStatus.Filled : ReceiptStatus.Manual;
                        receipt.CustomerId = data.SelectedCustomerId;
                        await _receiptService.SetCustomersToReceiptAsync(receipt.Id, data.SelectedConsumerIds);
                        await _receiptService.UpdateAsync(receipt);
                    }

                    break;
                }
                case MenuType.NewReceiptCancel:
                {
                    var data = (AddReceiptQueryData) queryData;
                    await EditMessageAsync(userMessageInfo, "Добавление чека было отменено");
                    Receipt receipt = await _receiptService.GetAsync(data.ReceiptId);
                    receipt.Status = ReceiptStatus.Deleted;
                    await _receiptService.UpdateAsync(receipt);
                    break;
                }
                case MenuType.MoneyTransferSelectFrom:
                {
                    var data = (MoneyTransferQueryData) queryData;
                    IMenu menu = _menuProvider.GetMenu(userMessageInfo, data);
                    await EditMessageAsync(userMessageInfo, "Кто переводит?", menu);
                    break;
                }
                case MenuType.MoneyTransferSelectTo:
                {
                    var data = (MoneyTransferQueryData) queryData;
                    IMenu menu = _menuProvider.GetMenu(userMessageInfo, data);
                    await EditMessageAsync(userMessageInfo, "Кому переводит?", menu);
                    break;
                }
                case MenuType.MoneyTransferAdd:
                {
                    var data = (MoneyTransferQueryData) queryData;
                    string customerFrom = userMessageInfo.Customers.FirstOrDefault(x => data.CustomerFromId == x.Id)?.Caption ?? $"Id: {data.CustomerFromId}";
                    string customerTo = userMessageInfo.Customers.FirstOrDefault(x => data.CustomerToId == x.Id)?.Caption ?? $"Id: {data.CustomerToId}";
                    await EditMessageAsync(userMessageInfo, $"Добавление перевода было успешно совершено.\nПеревёл: {customerFrom}\nКому: {customerTo}\nСумма: {data.Amount} руб.");

                    var lastBillingPeriod = await _billingPeriodService.GetLastByGroupIdAsync(userMessageInfo.Group.Id);
                    if (lastBillingPeriod == null)
                    {
                        await EditMessageAsync(userMessageInfo, "Нельзя добавить перевод денег, если не начат расчётный период");
                        break;
                    }

                    await _moneyOperationService.AddAsync(new MoneyOperation
                    {
                        Amount = data.Amount,
                        Comment = data.Caption,
                        CustomerFromId = data.CustomerFromId.Value,
                        CustomerToId = data.CustomerToId.Value,
                        OperationType = MoneyOperationType.Transfer,
                        BillingPeriodId = lastBillingPeriod.Id
                    });
                    break;
                }
                case MenuType.MoneyTransferCancel:
                {
                    await EditMessageAsync(userMessageInfo, "Добавление перевода было отменено");
                    break;
                }
            }
        }

        public async Task EditMessageAsync(UserMessageInfo userMessageInfo, string text, IMenu menu = null)
        {
            await _client.EditMessageTextAsync(userMessageInfo.Group.ChatToken, int.Parse(userMessageInfo.Message.Token), text, replyMarkup: (InlineKeyboardMarkup) ((TelegramMenu) menu)?.Markup);
        }

        private async void OnMessageReceived(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            if (e.Message.Chat.Type != ChatType.Group)
                return;

            long chatId = e.Message.Chat.Id;

            if (chatId.ToString() != _cashogSettings.AdminChatToken)
            {
                _logger.Info($"Произведена попытка использования бота в группе `{e.Message.Chat.Title}` ({chatId})");
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
                    QrCode = null,
                    Text = e.Message.Text,
                }
            };

            // Дописываем дополнительную инфу.
            switch (e.Message.Type)
            {
                case MessageType.Photo:
                {
                    PhotoSize photoSize = e.Message.Photo.OrderByDescending(x => x.Width).First();
                    _logger.Trace($"Получено фото чека с разрешением {photoSize.Width}x{photoSize.Height}");

                    File file = await _client.GetFileAsync(photoSize.FileId);

                    using (MemoryStream clientStream = new MemoryStream())
                    {
                        await _client.DownloadFileAsync(file.FilePath, clientStream);

                        // Получаем изображение из потока.
                        Bitmap returnImage = (Bitmap) Image.FromStream(clientStream);
                        userMessageInfo.Message.QrCode = await _receiptHandleService.ParsePhotoAsync(returnImage);
                        userMessageInfo.MessageType = Core.Models.MessageType.QrCode;
                    }

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

        public event EventHandler<UserMessageInfo> OnMessage;

        public async Task SendMessageAsync(UserMessageInfo userMessageInfo, string text, bool isReply = false, IMenu menu = null)
        {
            var replyToMessageId = isReply ? int.Parse(userMessageInfo.Message.Token) : 0;

            if (menu != null && menu.GetType() != typeof(TelegramMenu))
                throw new ArgumentException($"{GetType().Name} может работать только с {nameof(IMenu)} типа {nameof(TelegramMenu)}");

            var telegramMenu = menu as TelegramMenu;
            await _client.SendTextMessageAsync(userMessageInfo.Group.ChatToken, text, replyToMessageId: replyToMessageId, replyMarkup: telegramMenu?.Markup);
        }
    }
}