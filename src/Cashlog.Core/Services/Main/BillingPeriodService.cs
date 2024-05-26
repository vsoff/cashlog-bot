using Cashlog.Core.Mappers;
using Cashlog.Core.Models.Main;
using Cashlog.Core.Services.Abstract;
using Cashlog.Data;
using Cashlog.Data.UoW;

namespace Cashlog.Core.Services.Main;

public class BillingPeriodService : IBillingPeriodService
{
    private readonly IDatabaseContextProvider _databaseContextProvider;

    public BillingPeriodService(IDatabaseContextProvider databaseContextProvider)
    {
        _databaseContextProvider =
            databaseContextProvider ?? throw new ArgumentNullException(nameof(databaseContextProvider));
    }

    public async Task<BillingPeriodDto> AddAsync(BillingPeriodDto item)
    {
        using (var uow = new UnitOfWork(_databaseContextProvider.Create()))
        {
            var operation = await uow.BillingPeriods.AddAsync(item.ToData());
            await uow.SaveChangesAsync();
            return operation?.ToCore();
        }
    }

    public async Task<BillingPeriodDto> UpdateAsync(BillingPeriodDto item)
    {
        using (var uow = new UnitOfWork(_databaseContextProvider.Create()))
        {
            var operation = await uow.BillingPeriods.UpdateAsync(item.ToData());
            await uow.SaveChangesAsync();
            return operation?.ToCore();
        }
    }

    public async Task<BillingPeriodDto> GetLastByGroupIdAsync(long groupId)
    {
        using (var uow = new UnitOfWork(_databaseContextProvider.Create()))
        {
            var operation = await uow.BillingPeriods.GetLastByGroupIdAsync(groupId);
            return operation?.ToCore();
        }
    }

    public async Task<BillingPeriodDto> GetAsync(long billingPeriodId)
    {
        using (var uow = new UnitOfWork(_databaseContextProvider.Create()))
        {
            return (await uow.BillingPeriods.GetAsync(billingPeriodId))?.ToCore();
        }
    }
}