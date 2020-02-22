using System.Collections.Generic;
using System.Threading.Tasks;
using Cashlog.Core.Models.Main;

namespace Cashlog.Core.Services.Abstract
{
    /// <summary>
    /// Сервис работы с чеками.
    /// </summary>
    public interface IReceiptService
    {
        Task<bool> IsReceiptExists(Receipt receipt);
        Task<Receipt> AddAsync(Receipt receipt);
        Task<Receipt> GetAsync(long receiptId);
        Task<Receipt> UpdateAsync(Receipt receipt);
        Task<Receipt[]> GetByBillingPeriodIdAsync(long billingPeriodId);
        Task SetCustomersToReceiptAsync(long receiptId, long[] consumerIds);
        Task<Dictionary<long, long[]>> GetConsumerIdsByReceiptIdsMapAsync(long[] receiptIds);
    }
}