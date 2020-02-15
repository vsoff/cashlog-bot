using System;
using System.Linq;
using System.Threading.Tasks;
using Cashlog.Core.Core.Models;
using Cashlog.Core.Core.Providers;
using Cashlog.Core.Core.Providers.Abstract;
using Cashlog.Core.Data.Mappers;
using Cashlog.Data.Entities;
using Cashlog.Data.UoW;

namespace Cashlog.Core.Core.Services.Abstract
{
    public class MoneyOperationService : IMoneyOperationService
    {
        private readonly IDatabaseContextProvider _databaseContextProvider;

        public MoneyOperationService(IDatabaseContextProvider databaseContextProvider)
        {
            _databaseContextProvider = databaseContextProvider ?? throw new ArgumentNullException(nameof(databaseContextProvider));
        }

        public async Task<MoneyOperation> AddAsync(MoneyOperation item)
        {
            using (var uow = new UnitOfWork(_databaseContextProvider.Create()))
            {
                MoneyOperationDto operation = await uow.MoneyOperations.AddAsync(item.ToData());
                await uow.SaveChangesAsync();
                return operation.ToCore();
            }
        }

        public async Task<MoneyOperation[]> AddAsync(MoneyOperation[] items)
        {
            using (var uow = new UnitOfWork(_databaseContextProvider.Create()))
            {
                MoneyOperationDto[] operations = await uow.MoneyOperations.AddRangeAsync(items.Select(x => x.ToData()));
                await uow.SaveChangesAsync();
                return operations.Select(x => x.ToCore()).ToArray();
            }
        }

        public async Task<MoneyOperation[]> GetByBillingPeriodIdAsync(long billingPeriodId)
        {
            using (var uow = new UnitOfWork(_databaseContextProvider.Create()))
            {
                MoneyOperationDto[] operations = await uow.MoneyOperations.GetByBillingPeriodIdAsync(billingPeriodId);
                return operations.Select(x => x.ToCore()).ToArray();
            }
        }
    }
}