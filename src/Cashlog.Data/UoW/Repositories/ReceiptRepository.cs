using Cashlog.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cashlog.Data.UoW.Repositories;

public interface IReceiptRepository : IRepository<ReceiptDto>
{
    Task<ReceiptDto[]> GetByBillingPeriodIdAsync(long billingPeriodId);
}

public class ReceiptRepository : Repository<ReceiptDto>, IReceiptRepository
{
    public ReceiptRepository(ApplicationContext context) : base(context)
    {
    }

    public async Task<ReceiptDto[]> GetByBillingPeriodIdAsync(long billingPeriodId)
    {
        return await Context.Set<ReceiptDto>().Where(x => x.BillingPeriodId == billingPeriodId).ToArrayAsync();
    }
}