using System.Threading.Tasks;
using Cashlog.Core.Core.Models;

namespace Cashlog.Core.Core.Services
{
    public interface IGroupService
    {
        Task<Group> AddAsync(string chatToken, string adminToken, string chatName);
        Task<Group> GetByChatTokenAsync(string chatToken);
    }
}