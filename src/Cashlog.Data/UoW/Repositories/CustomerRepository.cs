using Cashlog.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Cashlog.Data.UoW.Repositories
{
    public interface ICustomerRepository : IRepository<CustomerDto>
    {
        Task<CustomerDto[]> GetByGroupId(long groupId);
    }

    public class CustomerRepository : Repository<CustomerDto>, ICustomerRepository
    {
        public CustomerRepository(ApplicationContext context) : base(context)
        {
        }

        public async Task<CustomerDto[]> GetByGroupId(long groupId)
            => await Context.Set<CustomerDto>().Where(x => x.GroupId == groupId).ToArrayAsync();
    }
}