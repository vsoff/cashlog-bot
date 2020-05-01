using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Cashlog.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cashlog.Data.UoW
{
    public class Repository<T> : IRepository<T> where T : Entity
    {
        private readonly Expression<Func<T, bool>> _defaultExpression = x => true;

        protected readonly ApplicationContext Context;

        public Repository(ApplicationContext context)
        {
            Context = context;
        }

        public Task<T> GetAsync(long id)
            => Context.Set<T>().FirstOrDefaultAsync(x => x.Id == id);

        public Task<T> GetAsync(Expression<Func<T, bool>> whereExpression = null)
            => Context.Set<T>().FirstOrDefaultAsync(whereExpression ?? _defaultExpression);

        public Task<T[]> GetListAsync(long[] ids)
            => Context.Set<T>().Where(x => ids.Contains(x.Id)).ToArrayAsync();

        public Task<T[]> GetListAsync(PartitionRequest partitionRequest, Expression<Func<T, bool>> whereExpression = null)
            => Context.Set<T>().Where(whereExpression ?? _defaultExpression).Skip(partitionRequest.Skip).Take(partitionRequest.Take).ToArrayAsync();

        [Obsolete("Использовать другую перегрузку метода")]
        public async Task<ICollection<T>> GetListAsync(Expression<Func<T, bool>> whereExpression)
            => await Context.Set<T>().Where(whereExpression).ToListAsync();

        public Task<T[]> GetAllAsync()
            => Context.Set<T>().ToArrayAsync();

        public Task<bool> AnyAsync()
            => Context.Set<T>().AnyAsync();

        public async Task<T> AddAsync(T item)
            => (await AddRangeAsync(new[] {item})).First();

        public async Task<T[]> AddRangeAsync(IEnumerable<T> items)
        {
            List<T> newItems = new List<T>();

            foreach (var item in items)
            {
                item.CreatedAt = item.CreatedAt == DateTime.MinValue ? DateTime.UtcNow : item.CreatedAt;
                item.UpdatedAt = item.UpdatedAt == DateTime.MinValue ? DateTime.UtcNow : item.UpdatedAt;
                var newItem = (await Context.Set<T>().AddAsync(item)).Entity;
                Context.Entry(newItem).State = EntityState.Added;

                newItems.Add(newItem);
            }

            return newItems.ToArray();
        }

        public async Task<T> DeleteAsync(long id)
        {
            var item = await Context.Set<T>().FindAsync(id);
            Context.Entry(item).State = EntityState.Deleted;
            return item;
        }

        public async Task<T> UpdateAsync(T item)
        {
            var oldObj = await Context.Set<T>().FirstOrDefaultAsync(x => item.Id == x.Id);
            if (oldObj == null)
                throw new NullReferenceException("Невозможно обновить объект, которого нету в БД");

            Context.Entry(oldObj).State = EntityState.Detached;
            item.CreatedAt = oldObj.CreatedAt;
            item.UpdatedAt = DateTime.Now;
            Context.Entry(item).State = EntityState.Modified;
            return item;
        }
    }
}