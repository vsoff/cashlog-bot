using System;
using Cashlog.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Cashlog.Data.UoW.Repositories
{
    public interface IReceiptRepository : IRepository<ReceiptDto>
    {
        Task<ReceiptDto[]> GetByGroupIdAsync(long groupId);
        Task<ReceiptDto[]> GetAllFilteredAsync(long groupId, DateTime begin, DateTime end);
    }

    public class ReceiptRepository : Repository<ReceiptDto>, IReceiptRepository
    {
        public ReceiptRepository(ApplicationContext context) : base(context)
        {
        }

        public async Task<ReceiptDto[]> GetByGroupIdAsync(long groupId)
            => await Context.Set<ReceiptDto>().Where(x => x.GroupId == groupId).ToArrayAsync();

        public async Task<ReceiptDto[]> GetAllFilteredAsync(long groupId, DateTime begin, DateTime end)
            => await Context.Set<ReceiptDto>().Where(x => x.GroupId == groupId && x.PurchaseTime >= begin && x.PurchaseTime < end).ToArrayAsync();
    }
}