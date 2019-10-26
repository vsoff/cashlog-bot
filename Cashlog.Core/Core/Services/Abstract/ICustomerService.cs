using System.Threading.Tasks;
using Cashlog.Core.Core.Models;

namespace Cashlog.Core.Core.Services.Abstract
{
    public interface ICustomerService
    {
        Task<Customer> AddAsync(Customer customer);
        Task<Customer> GetAsync(long customerId);
        Task<Customer[]> GetByGroupIdAsync(long groupId);
    }
}