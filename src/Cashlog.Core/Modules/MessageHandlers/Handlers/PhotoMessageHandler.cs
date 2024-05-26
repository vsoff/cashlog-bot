using Cashlog.Common;
using Cashlog.Core.Common;
using Cashlog.Core.Models;
using Cashlog.Core.Models.Main;
using Cashlog.Core.Modules.Messengers;
using Cashlog.Core.Modules.Messengers.Menu;
using Cashlog.Core.Services.Abstract;
using Microsoft.Extensions.Logging;

namespace Cashlog.Core.Modules.MessageHandlers.Handlers;

/// <summary>
///     Обработчик сообщения с фотографией QR-кода.
/// </summary>
public class PhotoMessageHandler : IMessageHandler
{
    private readonly IBillingPeriodService _billingPeriodService;
    private readonly IMenuProvider _menuProvider;
    private readonly IMessenger _messenger;
    private readonly ILogger<PhotoMessageHandler> _logger;
    private readonly IReceiptService _receiptService;

    public PhotoMessageHandler(
        IBillingPeriodService billingPeriodService,
        IReceiptService receiptService,
        IMenuProvider menuProvider,
        IMessenger messenger,
        ILogger<PhotoMessageHandler> logger)
    {
        _billingPeriodService = billingPeriodService ?? throw new ArgumentNullException(nameof(billingPeriodService));
        _receiptService = receiptService ?? throw new ArgumentNullException(nameof(receiptService));
        _menuProvider = menuProvider ?? throw new ArgumentNullException(nameof(menuProvider));
        _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
        _logger = logger;
    }

    public MessageType MessageType => MessageType.QrCode;

    public async Task HandleAsync(UserMessageInfo userMessageInfo)
    {
        var data = userMessageInfo.Message.ReceiptInfo;

        if (data == null)
        {
            await _messenger.SendMessageAsync(userMessageInfo, Resources.ParsePhotoError, true);
            _logger.LogTrace(Resources.ParsePhotoError);
            return;
        }

        var lastBillingPeriod = await _billingPeriodService.GetLastByGroupIdAsync(userMessageInfo.Group.Id);
        if (lastBillingPeriod == null)
        {
            await _messenger.SendMessageAsync(userMessageInfo, Resources.BillingPeriodNotCreated, true);
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
            Comment = "Чек"
        };

        if (await _receiptService.IsReceiptExists(receipt))
        {
            await _messenger.SendMessageAsync(userMessageInfo, Resources.ReceiptAlredyAdded, true);
            return;
        }

        var newReceipt = await _receiptService.AddAsync(receipt);

        var menu = _menuProvider.GetMenu(userMessageInfo, new AddReceiptQueryData
        {
            MenuType = MenuType.NewReceiptSelectCustomer,
            ReceiptId = newReceipt.Id,
            SelectedCustomerId = null,
            SelectedConsumerIds = [],
            TargetId = null,
            Version = AddReceiptQueryData.ServerVersion
        });
        await _messenger.SendMessageAsync(userMessageInfo, Resources.SelectCustomer, true, menu);
    }
}