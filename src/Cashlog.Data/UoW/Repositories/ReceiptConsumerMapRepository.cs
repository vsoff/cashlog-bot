using Cashlog.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cashlog.Data.UoW.Repositories;

public interface IReceiptConsumerMapRepository : IRepository<ReceiptConsumerMap>
{
    Task<Dictionary<long, long[]>> GetConsumerIdsByReceiptIdsMapAsync(long[] receiptIds);
}

public class ReceiptConsumerMapRepository : Repository<ReceiptConsumerMap>, IReceiptConsumerMapRepository
{
    public ReceiptConsumerMapRepository(ApplicationContext context) : base(context)
    {
    }

    public async Task<Dictionary<long, long[]>> GetConsumerIdsByReceiptIdsMapAsync(long[] receiptIds)
    {
        var maps = await Context.Set<ReceiptConsumerMap>().Where(x => receiptIds.Contains(x.ReceiptId))
            .ToArrayAsync();
        return maps.GroupBy(x => x.ReceiptId).ToDictionary(x => x.Key, x => x.Select(y => y.ConsumerId).ToArray());
    }
}