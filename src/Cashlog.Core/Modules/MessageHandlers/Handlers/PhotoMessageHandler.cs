using Cashlog.Common;
using Cashlog.Core.Common;
using Cashlog.Core.Models;
using Cashlog.Core.Models.Main;
using Cashlog.Core.Modules.Messengers;
using Cashlog.Core.Modules.Messengers.Menu;
using Cashlog.Core.Services.Abstract;

namespace Cashlog.Core.Modules.MessageHandlers.Handlers;

/// <summary>
///     Обработчик сообщения с фотографией QR-кода.
/// </summary>
public class PhotoMessageHandler : IMessageHandler
{
    private readonly IBillingPeriodService _billingPeriodService;
    protected readonly ILogger Logger;
    protected readonly IMenuProvider MenuProvider;
    protected readonly IMessenger Messenger;
    protected readonly IReceiptService ReceiptService;

    public PhotoMessageHandler(
        IBillingPeriodService billingPeriodService,
        IReceiptService receiptService,
        IMenuProvider menuProvider,
        IMessenger messenger,
        ILogger logger)
    {
        _billingPeriodService = billingPeriodService ?? throw new ArgumentNullException(nameof(billingPeriodService));
        ReceiptService = receiptService ?? throw new ArgumentNullException(nameof(receiptService));
        MenuProvider = menuProvider ?? throw new ArgumentNullException(nameof(menuProvider));
        Messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public MessageType MessageType => MessageType.QrCode;

    public async Task HandleAsync(UserMessageInfo userMessageInfo)
    {
        var data = userMessageInfo.Message.ReceiptInfo;

        if (data == null)
        {
            await Messenger.SendMessageAsync(userMessageInfo, Resources.ParsePhotoError, true);
            Logger.Trace(Resources.ParsePhotoError);
            return;
        }

        var lastBillingPeriod = await _billingPeriodService.GetLastByGroupIdAsync(userMessageInfo.Group.Id);
        if (lastBillingPeriod == null)
        {
            await Messenger.SendMessageAsync(userMessageInfo, Resources.BillingPeriodNotCreated, true);
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

        if (await ReceiptService.IsReceiptExists(receipt))
        {
            await Messenger.SendMessageAsync(userMessageInfo, Resources.ReceiptAlredyAdded, true);
            return;
        }

        var newReceipt = await ReceiptService.AddAsync(receipt);

        var menu = MenuProvider.GetMenu(userMessageInfo, new AddReceiptQueryData
        {
            MenuType = MenuType.NewReceiptSelectCustomer,
            ReceiptId = newReceipt.Id,
            SelectedCustomerId = null,
            SelectedConsumerIds = new long[0],
            TargetId = null,
            Version = AddReceiptQueryData.ServerVersion
        });
        await Messenger.SendMessageAsync(userMessageInfo, Resources.SelectCustomer, true, menu);
    }
}