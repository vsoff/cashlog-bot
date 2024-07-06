using System.Text;
using Cashlog.Common;
using Cashlog.Common.Models;
using Cashlog.Core.Services.Abstract;
using Cashlog.Messenger;

namespace Cashlog.Core.MessageHandlers.Handlers.Text;

internal sealed class ReportMessagesHandler : TextCommandMessageHandler
{
    private readonly ICustomerService _customerService;
    private readonly IMessenger _messenger;
    private readonly IReceiptService _receiptService;

    public ReportMessagesHandler(
        ICustomerService customerService,
        IReceiptService receiptService,
        IMessenger messenger)
    {
        _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        _receiptService = receiptService ?? throw new ArgumentNullException(nameof(receiptService));
        _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
    }

    public override string Command => "report";

    protected override Task<bool> ValidateAsync(UserMessageInfo userMessageInfo, string argument)
    {
        // TODO
        return Task.FromResult(true);
    }

    protected override async Task HandleAsync(UserMessageInfo userMessageInfo, string argument)
    {
        var args = argument.Split(' ').ToArray();
        if (args.Length != 2)
        {
            await _messenger.SendMessageAsync(userMessageInfo, Resources.CommandWrongArgsCount, true);
            return;
        }

        if (!DateTime.TryParse(args[0], out var periodBegin) || !DateTime.TryParse(args[1], out var periodEnd))
        {
            await _messenger.SendMessageAsync(userMessageInfo, "Первый и второй аргумент должны быть датами", true);
            return;
        }

        await HandleAsync(userMessageInfo, periodBegin, periodEnd);
    }

    private async Task HandleAsync(UserMessageInfo userMessageInfo, DateTime periodBegin, DateTime periodEnd)
    {
        if (periodEnd - periodBegin < TimeSpan.FromDays(1))
        {
            await _messenger.SendMessageAsync(userMessageInfo, "Период дней не должен быть меньше дня", true);
            return;
        }

        var receipts = await _receiptService.GetReceiptsInPeriodAsync(periodBegin, periodEnd, userMessageInfo.Group.Id);
        var summary = receipts.Sum(x => x.TotalAmount);

        // Формируем сообщение.
        var customersMap = receipts
            .Where(x => x.CustomerId.HasValue)
            .GroupBy(x => x.CustomerId!.Value)
            .ToDictionary(x => x.Key, x => x.ToArray());
        var customersNameMap = (await _customerService.GetListAsync(customersMap.Keys.ToArray()))
            .ToDictionary(x => x.Id, x => x.Caption);
        var receiptLines = receipts.Select(x =>
                $"ID {x.Id}: [{x.TotalAmount}р.] {x.Comment ?? "(Нет описания)"} ({customersNameMap[x.CustomerId.Value]})")
            .ToArray();

        var sb = new StringBuilder();
        sb.AppendLine(
            $"Траты за период с {periodBegin.ToShortDateString()} по {periodEnd.ToShortDateString()} составляют: {summary:F2}р.");
        sb.AppendLine();
        sb.AppendLine("Список участников:");
        foreach (var customerKvp in customersMap)
            sb.AppendLine(
                $"* {customersNameMap[customerKvp.Key]} потратил {customerKvp.Value.Sum(x => x.TotalAmount):F2}р.");
        sb.AppendLine();
        sb.AppendLine("Список чеков:");
        sb.AppendLine($"{string.Join(";\n", receiptLines)}.");

        await _messenger.SendMessageAsync(userMessageInfo, sb.ToString(), true);
    }
}