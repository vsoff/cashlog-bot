using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Web;
using Cashlog.Core.Common;
using Cashlog.Core.Core.Models;
using Cashlog.Core.Core.Services;
using Cashlog.Core.Messengers;
using Cashlog.Core.Messengers.Menu;

namespace Cashlog.Core.Core
{
    public class MessagesHandler : IMessagesHandler
    {
        private readonly IReceiptHandleService _receiptHandleService;
        private readonly ICustomerService _customerService;
        private readonly IReceiptService _receiptService;
        private readonly IMenuProvider _menuProvider;
        private readonly IMessenger _messenger;
        private readonly ILogger _logger;

        public MessagesHandler(
            IReceiptHandleService receiptHandleService,
            ICustomerService customerService,
            IReceiptService receiptService,
            IMenuProvider menuProvider,
            IMessenger messenger,
            ILogger logger)
        {
            _receiptHandleService = receiptHandleService ?? throw new ArgumentNullException(nameof(receiptHandleService));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _receiptService = receiptService ?? throw new ArgumentNullException(nameof(receiptService));
            _menuProvider = menuProvider ?? throw new ArgumentNullException(nameof(menuProvider));
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _messenger.OnMessage += OnMessage;
        }

        private async void OnMessage(object sender, UserMessageInfo e)
        {
            switch (e.MessageType)
            {
                case MessageType.Text:
                    await HandleTextMessageAsync(e);
                    break;
                case MessageType.QrCode:
                    await HandlePhotoMessageAsync(e);
                    break;
                case MessageType.Command:
                    await HandleCommandMessageAsync(e);
                    break;
                default:
                    await _messenger.SendMessageAsync(e, "Неподдерживаемый тип сообщения", true);
                    break;
            }
        }

        private async Task HandleCommandMessageAsync(UserMessageInfo userMessageInfo)
        {
        }

        private async Task HandleTextMessageAsync(UserMessageInfo userMessageInfo)
        {
            _logger.Trace($"@{userMessageInfo.UserName} прислал сообщение");

            string text = userMessageInfo.Message.Text;
            if (text.StartsWith("/add") && text.Contains(" "))
            {
                var splitIndex = text.IndexOf(" ");
                var customerName = text.Substring(splitIndex);
                await _customerService.Add(new Customer
                {
                    Caption = customerName,
                    GroupId = userMessageInfo.Group.Id
                });

                await _messenger.SendMessageAsync(userMessageInfo, $"Добавлен новый потребитель: {customerName}", true);
                return;
            }

            await _messenger.SendMessageAsync(userMessageInfo, "Я не читаю сообщения, попробуйте прислать мне фото", true);
        }

        private async Task HandlePhotoMessageAsync(UserMessageInfo userMessageInfo)
        {
            QrCodeData data = userMessageInfo.Message.QrCode;

            if (data == null)
            {
                const string msg = "Не удалось прочитать чек";
                await _messenger.SendMessageAsync(userMessageInfo, msg, true);
                _logger.Trace(msg);
                return;
            }

            var newReceipt = await _receiptService.AddAsync(new Receipt
            {
                TotalAmount = data.TotalAmount,
                FiscalSign = data.FiscalSign,
                FiscalDocument = data.FiscalDocument,
                FiscalNumber = data.FiscalNumber,
                PurchaseTime = data.PurchaseTime,
                Status = ReceiptStatus.New,
                GroupId = userMessageInfo.Group.Id
            });

            IMenu menu = _menuProvider.BuildSelectCustomerMenu(userMessageInfo, newReceipt.Id);
            await _messenger.SendMessageAsync(userMessageInfo, $"Результат парсинга: {data.Data}\n\nКто оплатил чек?", true, menu);
        }

        public void Dispose()
        {
            _messenger.OnMessage -= OnMessage;
        }
    }
}