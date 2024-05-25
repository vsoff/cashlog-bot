using Cashlog.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cashlog.Data.UoW.Repositories;

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
    {
        return await Context.Set<CustomerDto>().Where(x => x.GroupId == groupId).ToArrayAsync();
    }
}