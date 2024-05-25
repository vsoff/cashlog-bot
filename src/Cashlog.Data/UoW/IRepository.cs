using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Cashlog.Data.Entities;

namespace Cashlog.Data.UoW
{
    public interface IRepository
    {
    }

    public interface IRepository<T> : IRepository where T : Entity
    {
        Task<T> GetAsync(long id);
        Task<T> GetAsync(Expression<Func<T, bool>> whereExpression);
        Task<T[]> GetListAsync(long[] ids);
        Task<ICollection<T>> GetListAsync(Expression<Func<T, bool>> whereExpression);
        Task<T> AddAsync(T item);
        Task<T[]> AddRangeAsync(IEnumerable<T> items);
        Task<T> UpdateAsync(T item);
        Task<T> DeleteAsync(long id);
        Task<T[]> GetAllAsync();
        Task<bool> AnyAsync(Expression<Func<T, bool>> whereExpression = null);
    }
}