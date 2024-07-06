using Cashlog.Common;
using Cashlog.Common.Models;
using Cashlog.Core.Extensions;
using Cashlog.Core.Services.Abstract;
using Cashlog.Messenger;

namespace Cashlog.Core.MessageHandlers.Handlers.Text;

internal sealed class PeriodMessagesHandler : TextCommandMessageHandler
{
    private readonly IMainLogicService _mainLogicService;
    private readonly IMessenger _messenger;

    public PeriodMessagesHandler(
        IMainLogicService mainLogicService,
        IMessenger messenger)
    {
        _mainLogicService = mainLogicService ?? throw new ArgumentNullException(nameof(mainLogicService));
        _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
    }

    public override string Command => "period";

    protected override async Task<bool> ValidateAsync(UserMessageInfo userMessageInfo, string argument)
    {
        if (!string.IsNullOrEmpty(argument))
        {
            await _messenger.SendMessageAsync(userMessageInfo, Resources.CommandWrongArgsCount, true);
            return false;
        }

        if (!userMessageInfo.IsAdmin())
        {
            await _messenger.SendMessageAsync(userMessageInfo,
                "Только создатель группы может создать новый расчётный период", true);
            return false;
        }

        return true;
    }

    protected override async Task HandleAsync(UserMessageInfo userMessageInfo, string argument)
    {
        var newPeriodResult = await _mainLogicService.CloseCurrentAndOpenNewPeriod(userMessageInfo.Group.Id);
        var namesMap = userMessageInfo.Customers.ToDictionary(x => x.Id, x => x.Caption);

        var debts = string.Join("\n", newPeriodResult.Debts
            .Select(x => $"* `{namesMap[x.CustomerFromId]}` должен отдать `{namesMap[x.CustomerToId]}` {x.Amount}р."));
        await _messenger.SendMessageAsync(userMessageInfo,
            $"Был начат новый период, долги за предыдущий составляют:\n{debts}", true);
    }
}