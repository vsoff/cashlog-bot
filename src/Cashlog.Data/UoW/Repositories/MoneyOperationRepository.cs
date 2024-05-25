using Cashlog.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cashlog.Data.UoW.Repositories;

public interface IMoneyOperationRepository : IRepository<MoneyOperationDto>
{
    Task<MoneyOperationDto[]> GetByBillingPeriodIdAsync(long billingPeriodId);
}

public class MoneyOperationRepository : Repository<MoneyOperationDto>, IMoneyOperationRepository
{
    public MoneyOperationRepository(ApplicationContext context) : base(context)
    {
    }

    public async Task<MoneyOperationDto[]> GetByBillingPeriodIdAsync(long billingPeriodId)
    {
        return await Context.MoneyOperations.Where(x => x.BillingPeriodId == billingPeriodId).ToArrayAsync();
    }
}