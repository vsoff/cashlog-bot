using Cashlog.Data.Entities;

namespace Cashlog.Data.UoW.Repositories
{
    public interface IBillingTimeRepository : IRepository<BillingTimeDto>
    {
    }

    public class BillingTimeRepository : Repository<BillingTimeDto>, IBillingTimeRepository
    {
        public BillingTimeRepository(ApplicationContext context) : base(context)
        {
        }
    }
}