using System.Threading.Tasks;
using Cashlog.Core.Core.Models;

namespace Cashlog.Core.Core.Services.Abstract
{
    public interface IBillingPeriodService
    {
        Task<BillingPeriod> AddAsync(BillingPeriod item);
        Task<BillingPeriod> GetAsync(long billingPeriodId);
        Task<BillingPeriod> UpdateAsync(BillingPeriod item);
        Task<BillingPeriod> GetLastByGroupIdAsync(long groupId);
    }
}