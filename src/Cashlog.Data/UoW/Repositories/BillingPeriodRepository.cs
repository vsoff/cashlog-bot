using System.Linq;
using System.Threading.Tasks;
using Cashlog.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cashlog.Data.UoW.Repositories
{
    public interface IBillingPeriodRepository : IRepository<BillingPeriodDto>
    {
        Task<BillingPeriodDto> GetLastByGroupIdAsync(long groupId);
    }

    public class BillingPeriodRepository : Repository<BillingPeriodDto>, IBillingPeriodRepository
    {
        public BillingPeriodRepository(ApplicationContext context) : base(context)
        {
        }

        public async Task<BillingPeriodDto> GetLastByGroupIdAsync(long groupId)
            => await Context.Set<BillingPeriodDto>().OrderByDescending(x => x.PeriodBegin).FirstOrDefaultAsync(x => x.GroupId == groupId);
    }
}