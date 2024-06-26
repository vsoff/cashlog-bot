﻿using Cashlog.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cashlog.Data.UoW.Repositories;

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
    {
        var maps = await Context.Set<ReceiptConsumerMapDto>().Where(x => receiptIds.Contains(x.ReceiptId))
            .ToArrayAsync();
        return maps.GroupBy(x => x.ReceiptId).ToDictionary(x => x.Key, x => x.Select(y => y.ConsumerId).ToArray());
    }
}