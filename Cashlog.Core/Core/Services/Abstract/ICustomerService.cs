using System.Threading.Tasks;
using Cashlog.Core.Core.Models;

namespace Cashlog.Core.Core.Services
{
    public interface ICustomerService
    {
        Task<Customer> Add(Customer customer);
        Task<Customer> Get(long customerId);
        Task<Customer[]> GetByGroupId(long groupId);
    }
}