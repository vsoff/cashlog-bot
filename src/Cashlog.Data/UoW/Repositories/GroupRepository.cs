using Cashlog.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cashlog.Data.UoW.Repositories;

public interface IGroupRepository : IRepository<Group>
{
    Task<Group> GetByChatTokenAsync(string chatToken);
}

public class GroupRepository : Repository<Group>, IGroupRepository
{
    public GroupRepository(ApplicationContext context) : base(context)
    {
    }

    public async Task<Group> GetByChatTokenAsync(string chatToken)
    {
        return await Context.Set<Group>().FirstOrDefaultAsync(x => x.ChatToken == chatToken);
    }
}