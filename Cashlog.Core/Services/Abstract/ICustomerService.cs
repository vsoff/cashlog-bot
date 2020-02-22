using System.Threading.Tasks;
using Cashlog.Core.Models.Main;

namespace Cashlog.Core.Services.Abstract
{
    /// <summary>
    /// Сервис управления потребителями.
    /// </summary>
    public interface ICustomerService
    {
        Task<Customer> AddAsync(Customer customer);
        Task<Customer> GetAsync(long customerId);
        Task<Customer[]> GetByGroupIdAsync(long groupId);
    }
}