using Cashlog.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cashlog.Data.UoW.Repositories;

public interface IMoneyOperationRepository : IRepository<MoneyOperation>
{
    Task<MoneyOperation[]> GetByBillingPeriodIdAsync(long billingPeriodId);
}

public class MoneyOperationRepository : Repository<MoneyOperation>, IMoneyOperationRepository
{
    public MoneyOperationRepository(ApplicationContext context) : base(context)
    {
    }

    public async Task<MoneyOperation[]> GetByBillingPeriodIdAsync(long billingPeriodId)
    {
        return await Context.MoneyOperations.Where(x => x.BillingPeriodId == billingPeriodId).ToArrayAsync();
    }
}