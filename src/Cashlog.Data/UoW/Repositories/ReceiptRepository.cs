using Cashlog.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cashlog.Data.UoW.Repositories;

public interface IReceiptRepository : IRepository<Receipt>
{
    Task<Receipt[]> GetByBillingPeriodIdAsync(long billingPeriodId);
}

public class ReceiptRepository : Repository<Receipt>, IReceiptRepository
{
    public ReceiptRepository(ApplicationContext context) : base(context)
    {
    }

    public async Task<Receipt[]> GetByBillingPeriodIdAsync(long billingPeriodId)
    {
        return await Context.Set<Receipt>().Where(x => x.BillingPeriodId == billingPeriodId).ToArrayAsync();
    }
}