using System.Collections.Generic;
using System.Threading.Tasks;
using Cashlog.Core.Core.Models;

namespace Cashlog.Core.Core.Services
{
    public interface IReceiptService
    {
        Task<Receipt> AddAsync(Receipt receipt);
        Task<Receipt> GetAsync(long receiptId);
        Task<Receipt> UpdateAsync(Receipt receipt);
        Task<Receipt[]> GetByBillingPeriodIdAsync(long billingPeriodId);
        Task SetCustomersToReceiptAsync(long receiptId, long[] consumerIds);
        Task<Dictionary<long, long[]>> GetConsumerIdsByReceiptIdsMapAsync(long[] receiptIds);
    }
}