using System.Collections.Generic;
using System.Threading.Tasks;
using Cashlog.Data.Entities;

namespace Cashlog.Data.UoW
{
    public interface IRepository<T> where T : Entity
    {
        Task<T> GetAsync(long id);
        Task<T[]> GetListAsync(long[] ids);
        Task<T> AddAsync(T item);
        Task<T[]> AddRangeAsync(IEnumerable<T> items);
        Task<T> UpdateAsync(T item);
        Task<T> DeleteAsync(long id);
    }
}