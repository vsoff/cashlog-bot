using System.Threading.Tasks;
using Cashlog.Core.Core.Models;

namespace Cashlog.Core.Core.Services
{
    public interface IReceiptService
    {
        Task<Receipt> AddAsync(Receipt receipt);
        Task<Receipt> GetAsync(long receiptId);
        Task<Receipt> UpdateAsync(Receipt receipt);
        Task<Receipt[]> GetByGroupIdAsync(long groupId);
        Task SetCustomersToReceiptAsync(long receiptId, long[] consumerIds);
    }
}