using Cashlog.Common;
using Cashlog.Core.Models.Main;
using Cashlog.Core.Modules.Calculator;
using Cashlog.Core.Services.Abstract;

namespace Cashlog.Core.Services;

public class MainLogicService : IMainLogicService
{
    private readonly IBillingPeriodService _billingPeriodService;
    private readonly IDebtsCalculator _debtsCalculator;
    private readonly IMoneyOperationService _moneyOperationService;
    private readonly IReceiptService _receiptService;

    public MainLogicService(
        IDebtsCalculator debtsCalculator,
        IMoneyOperationService moneyOperationService,
        IBillingPeriodService billingPeriodService,
        IReceiptService receiptService)
    {
        _debtsCalculator = debtsCalculator ?? throw new ArgumentNullException(nameof(debtsCalculator));
        _moneyOperationService =
            moneyOperationService ?? throw new ArgumentNullException(nameof(moneyOperationService));
        _billingPeriodService = billingPeriodService ?? throw new ArgumentNullException(nameof(billingPeriodService));
        _receiptService = receiptService ?? throw new ArgumentNullException(nameof(receiptService));
    }

    public async Task<MoneyOperationShortInfo[]> CalculatePeriodCurrentDebts(long billingPeriodId)
    {
        var periodOperations = await _moneyOperationService.GetByBillingPeriodIdAsync(billingPeriodId);
        var periodReceipts = (await _receiptService.GetByBillingPeriodIdAsync(billingPeriodId))
            .Where(x => x.Status.IsFinalStatus()).ToArray();
        var consumerMap =
            await _receiptService.GetConsumerIdsByReceiptIdsMapAsync(periodReceipts.Select(x => x.Id).ToArray());

        return await _debtsCalculator
            .Calculate(periodOperations, periodReceipts
                .Where(x => x.CustomerId.HasValue)
                .Select(x => new ReceiptCalculatorInfo
                {
                    Amount = x.TotalAmount,
                    CustomerId = x.CustomerId.Value,
                    ConsumerIds = consumerMap[x.Id]
                }).ToArray());
    }

    public async Task<ClosingPeriodResult> CloseCurrentAndOpenNewPeriod(long groupId)
    {
        var now = DateTime.Now;

        // Закрываем старый период.
        var lastBillingPeriod = await _billingPeriodService.GetLastByGroupIdAsync(groupId);
        if (lastBillingPeriod != null)
        {
            lastBillingPeriod.PeriodEnd = now;
            await _billingPeriodService.UpdateAsync(lastBillingPeriod);
        }

        // Создаём новый период.
        var newBillingPeriod = await _billingPeriodService.AddAsync(new BillingPeriodDto
        {
            GroupId = groupId,
            PeriodBegin = now,
            PeriodEnd = null
        });

        // Рассчитываем затраты за предыдущий период.
        MoneyOperationDto[] debts = null;
        if (lastBillingPeriod != null)
        {
            // Записываем задолжности за предыдущий расчётный период.
            var debtsShortInfo = await CalculatePeriodCurrentDebts(lastBillingPeriod.Id);
            debts = debtsShortInfo.Select(x => new MoneyOperationDto
            {
                Amount = (int)x.Amount,
                BillingPeriodId = newBillingPeriod.Id,
                Comment = "Долг с предыдущего периода",
                CustomerFromId = x.FromId,
                CustomerToId = x.ToId,
                OperationType = MoneyOperationType.Debt
            }).ToArray();
            await _moneyOperationService.AddAsync(debts);
        }

        return new ClosingPeriodResult
        {
            Debts = debts ?? [],
            PreviousPeriod = lastBillingPeriod,
            NewPeriod = newBillingPeriod
        };
    }
}