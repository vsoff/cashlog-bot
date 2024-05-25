using System.Threading.Tasks;
using Cashlog.Core.Models.Main;

namespace Cashlog.Core.Services.Abstract
{
    /// <summary>
    /// Сервис управления расчётными периодами.
    /// </summary>
    public interface IBillingPeriodService
    {
        Task<BillingPeriod> AddAsync(BillingPeriod item);
        Task<BillingPeriod> GetAsync(long billingPeriodId);
        Task<BillingPeriod> UpdateAsync(BillingPeriod item);
        Task<BillingPeriod> GetLastByGroupIdAsync(long groupId);
    }
}