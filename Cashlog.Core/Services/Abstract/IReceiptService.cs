using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cashlog.Core.Models.Main;
using Cashlog.Data;

namespace Cashlog.Core.Services.Abstract
{
    /// <summary>
    /// Сервис работы с чеками.
    /// </summary>
    public interface IReceiptService
    {
        Task<ICollection<Receipt>> GetReceiptsInPeriodAsync(DateTime periodFrom, DateTime periodTo, long groupId);
        Task<bool> IsReceiptExists(Receipt receipt);
        Task<Receipt> AddAsync(Receipt receipt);
        Task<Receipt> GetAsync(long receiptId);
        Task<Receipt> UpdateAsync(Receipt receipt);
        Task<Receipt[]> GetListAsync(PartitionRequest partitionRequest);
        Task<Receipt[]> GetByBillingPeriodIdAsync(long billingPeriodId);
        Task SetCustomersToReceiptAsync(long receiptId, long[] consumerIds);
        Task<Dictionary<long, long[]>> GetConsumerIdsByReceiptIdsMapAsync(long[] receiptIds);
    }
}