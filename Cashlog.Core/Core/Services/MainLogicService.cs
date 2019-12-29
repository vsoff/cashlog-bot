using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cashlog.Core.Core.Models;
using Cashlog.Core.Core.Services.Abstract;
using Cashlog.Core.Modules.Calculator;

namespace Cashlog.Core.Core.Services
{
    public interface IMainLogicService
    {
        /// <summary>
        /// Рассчитывает долги для расчётного периода.
        /// </summary>
        Task<MoneyOperationShortInfo[]> CalculatePeriodCurrentDebts(long billingPeriodId);

        /// <summary>
        /// Закрывает расчётный период и открывает новый.
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        Task<ClosingPeriodResult> CloseCurrentAndOpenNewPeriod(long groupId);
    }

    public class MainLogicService : IMainLogicService
    {
        private readonly IDebtsCalculator _debtsCalculator;
        private readonly IMoneyOperationService _moneyOperationService;
        private readonly IBillingPeriodService _billingPeriodService;
        private readonly ICustomerService _customerService;
        private readonly IReceiptService _receiptService;

        public MainLogicService(
            IDebtsCalculator debtsCalculator,
            IMoneyOperationService moneyOperationService,
            IBillingPeriodService billingPeriodService,
            ICustomerService customerService,
            IReceiptService receiptService)
        {
            _debtsCalculator = debtsCalculator ?? throw new ArgumentNullException(nameof(debtsCalculator));
            _moneyOperationService = moneyOperationService ?? throw new ArgumentNullException(nameof(moneyOperationService));
            _billingPeriodService = billingPeriodService ?? throw new ArgumentNullException(nameof(billingPeriodService));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _receiptService = receiptService ?? throw new ArgumentNullException(nameof(receiptService));
        }

        public async Task<MoneyOperationShortInfo[]> CalculatePeriodCurrentDebts(long billingPeriodId)
        {
            MoneyOperation[] periodOperations = await _moneyOperationService.GetByBillingPeriodIdAsync(billingPeriodId);
            Receipt[] periodReceipts = (await _receiptService.GetByBillingPeriodIdAsync(billingPeriodId))
                .Where(x => x.Status.IsFinalStatus()).ToArray();
            Dictionary<long, long[]> consumerMap = await _receiptService.GetConsumerIdsByReceiptIdsMapAsync(periodReceipts.Select(x => x.Id).ToArray());

            return await _debtsCalculator.Calculate(periodOperations, periodReceipts.Where(x => x.CustomerId.HasValue).Select(x => new ReceiptCalculatorInfo
            {
                Amount = x.TotalAmount,
                CustomerId = x.CustomerId.Value,
                ConsumerIds = consumerMap[x.Id]
            }).ToArray());
        }

        public async Task<ClosingPeriodResult> CloseCurrentAndOpenNewPeriod(long groupId)
        {
            DateTime now = DateTime.Now;

            // Закрываем старый период.
            BillingPeriod lastBillingPeriod = await _billingPeriodService.GetLastByGroupIdAsync(groupId);
            if (lastBillingPeriod != null)
            {
                lastBillingPeriod.PeriodEnd = now;
                await _billingPeriodService.UpdateAsync(lastBillingPeriod);
            }

            // Создаём новый период.
            BillingPeriod newBillingPeriod = await _billingPeriodService.AddAsync(new BillingPeriod
            {
                GroupId = groupId,
                PeriodBegin = now,
                PeriodEnd = null
            });

            // Рассчитываем затраты за предыдущий период.
            MoneyOperation[] debts = null;
            if (lastBillingPeriod != null)
            {
                // Записываем задолжности за предыдущий расчётный период.
                MoneyOperationShortInfo[] debtsShortInfo = await CalculatePeriodCurrentDebts(lastBillingPeriod.Id);
                debts = debtsShortInfo.Select(x => new MoneyOperation
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
                Debts = debts ?? new MoneyOperation[0],
                PreviousPeriod = lastBillingPeriod,
                NewPeriod = newBillingPeriod
            };
        }
    }
}