using Cashlog.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cashlog.Data.UoW.Repositories;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer[]> GetByGroupId(long groupId);
}

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(ApplicationContext context) : base(context)
    {
    }

    public async Task<Customer[]> GetByGroupId(long groupId)
    {
        return await Context.Set<Customer>().Where(x => x.GroupId == groupId).ToArrayAsync();
    }
}