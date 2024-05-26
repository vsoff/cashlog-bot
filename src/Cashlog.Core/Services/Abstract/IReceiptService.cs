using Cashlog.Core.Models.Main;
using Cashlog.Data;

namespace Cashlog.Core.Services.Abstract;

/// <summary>
///     Сервис работы с чеками.
/// </summary>
public interface IReceiptService
{
    Task<ICollection<ReceiptDto>> GetReceiptsInPeriodAsync(DateTime periodFrom, DateTime periodTo, long groupId);
    Task<bool> IsReceiptExists(ReceiptDto receipt);
    Task<ReceiptDto> AddAsync(ReceiptDto receipt);
    Task<ReceiptDto> GetAsync(long receiptId);
    Task<ReceiptDto> UpdateAsync(ReceiptDto receipt);
    Task<ReceiptDto[]> GetListAsync(PartitionRequest partitionRequest);
    Task<ReceiptDto[]> GetByBillingPeriodIdAsync(long billingPeriodId);
    Task SetCustomersToReceiptAsync(long receiptId, long[] consumerIds);
    Task<Dictionary<long, long[]>> GetConsumerIdsByReceiptIdsMapAsync(long[] receiptIds);
}