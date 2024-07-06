using Cashlog.Common;
using Cashlog.Common.Models;
using Cashlog.Common.Models.Main;
using Cashlog.Core.Services.Abstract;
using Cashlog.Messenger;
using Cashlog.Messenger.Menu;

namespace Cashlog.Core.MessageHandlers.Handlers.Text;

internal sealed class ReceiptMessagesHandler : TextCommandMessageHandler
{
    private readonly IBillingPeriodService _billingPeriodService;
    private readonly IMenuProvider _menuProvider;
    private readonly IMessenger _messenger;
    private readonly IReceiptService _receiptService;

    public ReceiptMessagesHandler(
        IBillingPeriodService billingPeriodService,
        IReceiptService receiptService,
        IMenuProvider menuProvider,
        IMessenger messenger)
    {
        _billingPeriodService = billingPeriodService ?? throw new ArgumentNullException(nameof(billingPeriodService));
        _receiptService = receiptService ?? throw new ArgumentNullException(nameof(receiptService));
        _menuProvider = menuProvider ?? throw new ArgumentNullException(nameof(menuProvider));
        _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
    }

    public override string Command => "receipt";

    protected override Task<bool> ValidateAsync(UserMessageInfo userMessageInfo, string argument)
    {
        // TODO
        return Task.FromResult(true);
    }

    protected override async Task HandleAsync(UserMessageInfo userMessageInfo, string argument)
    {
        var args = argument.Split(' ').ToArray();
        const int argsCount = 1;
        if (args.Length <= argsCount)
        {
            await _messenger.SendMessageAsync(userMessageInfo, Resources.CommandWrongArgsCount, true);
            return;
        }

        if (!int.TryParse(args[0], out var money))
        {
            await _messenger.SendMessageAsync(userMessageInfo, Resources.CommandFirstArgMustBeANumber, true);
            return;
        }

        var caption = string.Join(" ", args.Skip(argsCount));

        const int commentMinLength = 2;
        const int commentMaxLength = 50;
        if (caption.Length < commentMinLength || caption.Length > commentMaxLength)
        {
            var msgText = string.Format(Resources.CommentLengthWrong, commentMinLength, commentMaxLength);
            await _messenger.SendMessageAsync(userMessageInfo, msgText, true);
            return;
        }

        await HandleAsync(userMessageInfo, money, caption);
    }

    private async Task HandleAsync(UserMessageInfo userMessageInfo, int amount, string caption)
    {
        var lastBillingPeriod = await _billingPeriodService.GetLastByGroupIdAsync(userMessageInfo.Group.Id);
        if (lastBillingPeriod == null)
        {
            await _messenger.SendMessageAsync(userMessageInfo, "В группе ещё не начат ни один расчётный период", true);
            return;
        }

        var newReceipt = await _receiptService.AddAsync(new ReceiptDto
        {
            GroupId = userMessageInfo.Group.Id,
            BillingPeriodId = lastBillingPeriod.Id,
            TotalAmount = amount,
            Status = ReceiptStatus.NewManual,
            Comment = caption
        });

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