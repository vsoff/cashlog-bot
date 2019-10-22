using Cashlog.Data.Entities;

namespace Cashlog.Data.UoW.Repositories
{
    public interface IReceiptConsumerMapRepository : IRepository<ReceiptConsumerMapDto>
    {
    }

    public class ReceiptConsumerMapRepository : Repository<ReceiptConsumerMapDto>, IReceiptConsumerMapRepository
    {
        public ReceiptConsumerMapRepository(ApplicationContext context) : base(context)
        {
        }
    }
}