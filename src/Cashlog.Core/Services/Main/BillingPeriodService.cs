using System;
using System.Threading.Tasks;
using Cashlog.Core.Mappers;
using Cashlog.Core.Models.Main;
using Cashlog.Core.Providers.Abstract;
using Cashlog.Core.Services.Abstract;
using Cashlog.Data.Entities;
using Cashlog.Data.UoW;

namespace Cashlog.Core.Services.Main
{
    public class BillingPeriodService : IBillingPeriodService
    {
        private readonly IDatabaseContextProvider _databaseContextProvider;

        public BillingPeriodService(IDatabaseContextProvider databaseContextProvider)
        {
            _databaseContextProvider = databaseContextProvider ?? throw new ArgumentNullException(nameof(databaseContextProvider));
        }

        public async Task<BillingPeriod> AddAsync(BillingPeriod item)
        {
            using (var uow = new UnitOfWork(_databaseContextProvider.Create()))
            {
                BillingPeriodDto operation = await uow.BillingPeriods.AddAsync(item.ToData());
                await uow.SaveChangesAsync();
                return operation?.ToCore();
            }
        }

        public async Task<BillingPeriod> UpdateAsync(BillingPeriod item)
        {
            using (var uow = new UnitOfWork(_databaseContextProvider.Create()))
            {
                BillingPeriodDto operation = await uow.BillingPeriods.UpdateAsync(item.ToData());
                await uow.SaveChangesAsync();
                return operation?.ToCore();
            }
        }

        public async Task<BillingPeriod> GetLastByGroupIdAsync(long groupId)
        {
            using (var uow = new UnitOfWork(_databaseContextProvider.Create()))
            {
                BillingPeriodDto operation = await uow.BillingPeriods.GetLastByGroupIdAsync(groupId);
                return operation?.ToCore();
            }
        }

        public async Task<BillingPeriod> GetAsync(long billingPeriodId)
        {
            using (var uow = new UnitOfWork(_databaseContextProvider.Create()))
            {
                return (await uow.BillingPeriods.GetAsync(billingPeriodId))?.ToCore();
            }
        }
    }
}