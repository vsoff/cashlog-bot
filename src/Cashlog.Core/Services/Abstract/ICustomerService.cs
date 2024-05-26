using Cashlog.Core.Models.Main;

namespace Cashlog.Core.Services.Abstract;

/// <summary>
///     Сервис управления потребителями.
/// </summary>
public interface ICustomerService
{
    Task<CustomerDto> AddAsync(CustomerDto customer);
    Task<CustomerDto> GetAsync(long customerId);
    Task<CustomerDto[]> GetListAsync(long[] customerIds);
    Task<CustomerDto[]> GetListAsync(long groupId);
}