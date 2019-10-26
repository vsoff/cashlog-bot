using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cashlog.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cashlog.Data.UoW.Repositories
{
    public interface IReceiptConsumerMapRepository : IRepository<ReceiptConsumerMapDto>
    {
        Task<Dictionary<long, long[]>> GetConsumerIdsByReceiptIdsMapAsync(long[] receiptIds);
    }

    public class ReceiptConsumerMapRepository : Repository<ReceiptConsumerMapDto>, IReceiptConsumerMapRepository
    {
        public ReceiptConsumerMapRepository(ApplicationContext context) : base(context)
        {
        }

        public async Task<Dictionary<long, long[]>> GetConsumerIdsByReceiptIdsMapAsync(long[] receiptIds)
            => await Context.Set<ReceiptConsumerMapDto>().Where(x => receiptIds.Contains(x.ReceiptId)).GroupBy(x => x.ReceiptId)
                .ToDictionaryAsync(x => x.Key, x => x.Select(y => y.ConsumerId).ToArray());
    }
}