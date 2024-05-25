using Cashlog.Core.Mappers;
using Cashlog.Core.Models.Main;
using Cashlog.Core.Providers.Abstract;
using Cashlog.Core.Services.Abstract;
using Cashlog.Data;
using Cashlog.Data.Entities;
using Cashlog.Data.UoW;

namespace Cashlog.Core.Services.Main;

public class ReceiptService : IReceiptService
{
    private readonly IDatabaseContextProvider _databaseContextProvider;

    public ReceiptService(IDatabaseContextProvider databaseContextProvider)
    {
        _databaseContextProvider =
            databaseContextProvider ?? throw new ArgumentNullException(nameof(databaseContextProvider));
    }

    public async Task<ICollection<Receipt>> GetReceiptsInPeriodAsync(DateTime periodFrom, DateTime periodTo,
        long groupId)
    {
        using var uow = new UnitOfWork(_databaseContextProvider.Create());
        var billingPeriods = await uow.BillingPeriods.GetListAsync(x => x.GroupId == groupId);
        var billingPeriodsIds = billingPeriods.Select(x => x.Id).ToHashSet();
        var receipts = await uow.Receipts.GetListAsync(x =>
            x.PurchaseTime >= periodFrom && x.PurchaseTime < periodTo && billingPeriodsIds.Contains(x.BillingPeriodId));
        return receipts.Select(x => x.ToCore()).ToList();
    }

    public async Task<bool> IsReceiptExists(Receipt receipt)
    {
        using var uow = new UnitOfWork(_databaseContextProvider.Create());
        return await IsReceiptExistsInternal(uow, receipt);
    }

    public async Task<Receipt> AddAsync(Receipt receipt)
    {
        using var uow = new UnitOfWork(_databaseContextProvider.Create());
        if (await IsReceiptExistsInternal(uow, receipt))
            throw new InvalidOperationException("Такой чек уже есть в БД");

        var newReceipt = await uow.Receipts.AddAsync(receipt.ToData());
        await uow.SaveChangesAsync();
        return newReceipt.ToCore();
    }

    public async Task<Receipt[]> GetListAsync(PartitionRequest partitionRequest)
    {
        using var uow = new UnitOfWork(_databaseContextProvider.Create());
        return (await uow.Receipts.GetListAsync(partitionRequest)).Select(x => x.ToCore()).ToArray();
    }

    public async Task<Receipt> GetAsync(long receiptId)
    {
        using var uow = new UnitOfWork(_databaseContextProvider.Create());
        return (await uow.Receipts.GetAsync(receiptId))?.ToCore();
    }

    public async Task<Receipt> UpdateAsync(Receipt receipt)
    {
        using var uow = new UnitOfWork(_databaseContextProvider.Create());
        var newReceipt = await uow.Receipts.UpdateAsync(receipt.ToData());
        await uow.SaveChangesAsync();
        return newReceipt.ToCore();
    }

    public async Task<Receipt[]> GetByBillingPeriodIdAsync(long billingPeriodId)
    {
        using var uow = new UnitOfWork(_databaseContextProvider.Create());
        return (await uow.Receipts.GetByBillingPeriodIdAsync(billingPeriodId))?.Select(x => x.ToCore()).ToArray();
    }

    public async Task SetCustomersToReceiptAsync(long receiptId, long[] consumerIds)
    {
        using var uow = new UnitOfWork(_databaseContextProvider.Create());
        await uow.ReceiptConsumerMaps.AddRangeAsync(consumerIds.Select(x => new ReceiptConsumerMapDto
        {
            ConsumerId = x,
            ReceiptId = receiptId
        }));
        await uow.SaveChangesAsync();
    }

    public async Task<Dictionary<long, long[]>> GetConsumerIdsByReceiptIdsMapAsync(long[] receiptIds)
    {
        using var uow = new UnitOfWork(_databaseContextProvider.Create());
        return await uow.ReceiptConsumerMaps.GetConsumerIdsByReceiptIdsMapAsync(receiptIds);
    }

    /// <summary>
    ///     Возвращает true, если такой чек уже присутствует в БД.
    /// </summary>
    private async Task<bool> IsReceiptExistsInternal(UnitOfWork uow, Receipt receipt)
    {
        if (string.IsNullOrEmpty(receipt.FiscalDocument))
            return false;

        var isExists = await uow.Receipts.AnyAsync(x => x.FiscalDocument == receipt.FiscalDocument);
        return isExists;
    }
}