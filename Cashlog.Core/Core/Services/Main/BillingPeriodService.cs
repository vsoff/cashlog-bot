using System;
using System.Threading.Tasks;
using Cashlog.Core.Core.Models;
using Cashlog.Core.Core.Services.Abstract;
using Cashlog.Core.Data.Mappers;
using Cashlog.Data.Entities;
using Cashlog.Data.UoW;

namespace Cashlog.Core.Core.Services
{
    public class BillingPeriodService : IBillingPeriodService
    {
        private readonly IMoneyOperationService _moneyOperationService;
        private readonly CashlogSettings _cashogSettings;

        public BillingPeriodService(
            IMoneyOperationService moneyOperationService,
            CashlogSettings cashogSettings)
        {
            _moneyOperationService = moneyOperationService ?? throw new ArgumentNullException(nameof(moneyOperationService));
            _cashogSettings = cashogSettings ?? throw new ArgumentNullException(nameof(cashogSettings));
        }

        public async Task<BillingPeriod> AddAsync(BillingPeriod item)
        {
            using (var uow = new UnitOfWork(_cashogSettings.DataBaseConnectionString))
            {
                BillingPeriodDto operation = await uow.BillingPeriods.AddAsync(item.ToData());
                await uow.SaveChangesAsync();
                return operation?.ToCore();
            }
        }

        public async Task<BillingPeriod> UpdateAsync(BillingPeriod item)
        {
            using (var uow = new UnitOfWork(_cashogSettings.DataBaseConnectionString))
            {
                BillingPeriodDto operation = await uow.BillingPeriods.UpdateAsync(item.ToData());
                await uow.SaveChangesAsync();
                return operation?.ToCore();
            }
        }

        public async Task<BillingPeriod> GetLastByGroupIdAsync(long groupId)
        {
            using (var uow = new UnitOfWork(_cashogSettings.DataBaseConnectionString))
            {
                BillingPeriodDto operation = await uow.BillingPeriods.GetLastByGroupIdAsync(groupId);
                return operation?.ToCore();
            }
        }

        public async Task<BillingPeriod> GetAsync(long billingPeriodId)
        {
            using (var uow = new UnitOfWork(_cashogSettings.DataBaseConnectionString))
            {
                return (await uow.BillingPeriods.GetAsync(billingPeriodId))?.ToCore();
            }
        }
    }
}