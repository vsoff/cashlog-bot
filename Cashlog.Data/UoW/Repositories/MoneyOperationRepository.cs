using Cashlog.Data.Entities;

namespace Cashlog.Data.UoW.Repositories
{
    public interface IMoneyOperationRepository : IRepository<MoneyOperationDto>
    {
    }

    public class MoneyOperationRepository : Repository<MoneyOperationDto>, IMoneyOperationRepository
    {
        public MoneyOperationRepository(ApplicationContext context) : base(context)
        {
        }
    }
}