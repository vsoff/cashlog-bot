using Cashlog.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cashlog.Data.UoW.Repositories;

public interface IBillingPeriodRepository : IRepository<BillingPeriod>
{
    Task<BillingPeriod> GetLastByGroupIdAsync(long groupId);
}

public class BillingPeriodRepository : Repository<BillingPeriod>, IBillingPeriodRepository
{
    public BillingPeriodRepository(ApplicationContext context) : base(context)
    {
    }

    public async Task<BillingPeriod> GetLastByGroupIdAsync(long groupId)
    {
        return await Context.Set<BillingPeriod>().OrderByDescending(x => x.PeriodBegin)
            .FirstOrDefaultAsync(x => x.GroupId == groupId);
    }
}