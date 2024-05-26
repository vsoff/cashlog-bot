using Cashlog.Core.Models.Main;

namespace Cashlog.Core.Services.Abstract;

/// <summary>
///     Сервис управления расчётными периодами.
/// </summary>
public interface IBillingPeriodService
{
    Task<BillingPeriodDto> AddAsync(BillingPeriodDto item);
    Task<BillingPeriodDto> GetAsync(long billingPeriodId);
    Task<BillingPeriodDto> UpdateAsync(BillingPeriodDto item);
    Task<BillingPeriodDto> GetLastByGroupIdAsync(long groupId);
}