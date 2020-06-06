using System;
using System.Threading.Tasks;
using Cashlog.Common;
using Cashlog.Core.Common;
using Cashlog.Core.Models;
using Cashlog.Core.Models.Main;
using Cashlog.Core.Modules.Messengers;
using Cashlog.Core.Modules.Messengers.Menu;
using Cashlog.Core.Services.Abstract;

namespace Cashlog.Core.Modules.MessageHandlers
{
    /// <summary>
    /// Обработчик сообщения с фотографией QR-кода.
    /// </summary>
    public class PhotoMessageHandler : IMessageHandler
    {
        public MessageType MessageType => MessageType.QrCode;

        private readonly IBillingPeriodService _billingPeriodService;
        protected readonly IReceiptService _receiptService;
        protected readonly IMenuProvider _menuProvider;
        protected readonly IMessenger _messenger;
        protected readonly ILogger _logger;

        public PhotoMessageHandler(
            IBillingPeriodService billingPeriodService,
            IReceiptService receiptService,
            IMenuProvider menuProvider,
            IMessenger messenger,
            ILogger logger)
        {
            _billingPeriodService = billingPeriodService ?? throw new ArgumentNullException(nameof(billingPeriodService));
            _receiptService = receiptService ?? throw new ArgumentNullException(nameof(receiptService));
            _menuProvider = menuProvider ?? throw new ArgumentNullException(nameof(menuProvider));
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleAsync(UserMessageInfo userMessageInfo)
        {
            ReceiptMainInfo data = userMessageInfo.Message.ReceiptInfo;

            if (data == null)
            {
                const string msg = "Не удалось прочитать чек";
                await _messenger.SendMessageAsync(userMessageInfo, msg, true);
                _logger.Trace(msg);
                return;
            }

            BillingPeriod lastBillingPeriod = await _billingPeriodService.GetLastByGroupIdAsync(userMessageInfo.Group.Id);
            if (lastBillingPeriod == null)
            {
                await _messenger.SendMessageAsync(userMessageInfo, "В группе ещё не начат ни один расчётный период", true);
                return;
            }

            var receipt = new Receipt
            {
                BillingPeriodId = lastBillingPeriod.Id,
                TotalAmount = data.TotalAmount,
                FiscalSign = data.FiscalSign,
                FiscalDocument = data.FiscalDocument,
                FiscalNumber = data.FiscalNumber,
                PurchaseTime = data.PurchaseTime,
                Status = ReceiptStatus.New,
                Comment = "Чек",
            };

            if (await _receiptService.IsReceiptExists(receipt))
            {
                await _messenger.SendMessageAsync(userMessageInfo, "Такой чек уже добавлен в БД", true);
                return;
            }

            var newReceipt = await _receiptService.AddAsync(receipt);

            IMenu menu = _menuProvider.GetMenu(userMessageInfo, new AddReceiptQueryData
            {
                MenuType = MenuType.NewReceiptSelectCustomer,
                ReceiptId = newReceipt.Id,
                SelectedCustomerId = null,
                SelectedConsumerIds = new long[0],
                TargetId = null,
                Version = AddReceiptQueryData.ServerVersion,
            });
            await _messenger.SendMessageAsync(userMessageInfo, "_Кто оплатил чек?", true, menu);
        }
    }
}