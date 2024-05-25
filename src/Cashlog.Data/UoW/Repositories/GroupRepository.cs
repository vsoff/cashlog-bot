using System.Linq;
using System.Threading.Tasks;
using Cashlog.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cashlog.Data.UoW.Repositories
{
    public interface IGroupRepository : IRepository<GroupDto>
    {
        Task<GroupDto> GetByChatTokenAsync(string chatToken);
    }

    public class GroupRepository : Repository<GroupDto>, IGroupRepository
    {
        public GroupRepository(ApplicationContext context) : base(context)
        {
        }

        public async Task<GroupDto> GetByChatTokenAsync(string chatToken)
            => await Context.Set<GroupDto>().FirstOrDefaultAsync(x => x.ChatToken == chatToken);
    }
}