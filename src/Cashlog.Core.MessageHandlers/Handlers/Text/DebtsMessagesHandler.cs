using Cashlog.Common;
using Cashlog.Common.Models;
using Cashlog.Core.Services.Abstract;
using Cashlog.Messenger;

namespace Cashlog.Core.MessageHandlers.Handlers.Text;

internal sealed class DebtsMessagesHandler : TextCommandMessageHandler
{
    private readonly IBillingPeriodService _billingPeriodService;
    private readonly IMainLogicService _mainLogicService;
    private readonly IMessenger _messenger;

    public DebtsMessagesHandler(
        IBillingPeriodService billingPeriodService,
        IMainLogicService mainLogicService,
        IMessenger messenger)
    {
        _billingPeriodService = billingPeriodService ?? throw new ArgumentNullException(nameof(billingPeriodService));
        _mainLogicService = mainLogicService ?? throw new ArgumentNullException(nameof(mainLogicService));
        _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
    }

    public override string Command => "debts";

    protected override async Task<bool> ValidateAsync(UserMessageInfo userMessageInfo, string argument)
    {
        if (!string.IsNullOrEmpty(argument))
        {
            await _messenger.SendMessageAsync(userMessageInfo, Resources.CommandWrongArgsCount, true);
            return false;
        }

        return true;
    }

    protected override async Task HandleAsync(UserMessageInfo userMessageInfo, string argument)
    {
        var currentBilling = await _billingPeriodService.GetLastByGroupIdAsync(userMessageInfo.Group.Id);
        var debts = await _mainLogicService.CalculatePeriodCurrentDebts(currentBilling.Id);

        var debtsMessages = debts.Select(x =>
        {
            var from = userMessageInfo.Customers.First(c => c.Id == x.FromId).Caption;
            var to = userMessageInfo.Customers.First(c => c.Id == x.ToId).Caption;
            return $"* {from} должен {to}: {(int)x.Amount}р.";
        }).ToArray();

        await _messenger.SendMessageAsync(userMessageInfo,
            $"Промежуточный итог долгов на период с {currentBilling.PeriodBegin}:\n{string.Join("\n", debtsMessages)}",
            true);
    }
}