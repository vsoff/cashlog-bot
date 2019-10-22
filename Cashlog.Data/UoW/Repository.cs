using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cashlog.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cashlog.Data.UoW
{
    public class Repository<T> : IRepository<T> where T : Entity
    {
        protected readonly ApplicationContext Context;

        public Repository(ApplicationContext context)
        {
            Context = context;
        }

        public async Task<T[]> GetListAsync(long[] ids)
            => await Context.Set<T>().Where(x => ids.Contains(x.Id)).ToArrayAsync();

        public async Task<T> AddAsync(T item)
            => (await AddRangeAsync(new[] {item})).First();

        public async Task<T[]> AddRangeAsync(IEnumerable<T> items)
        {
            List<T> newItems = new List<T>();

            foreach (var item in items)
            {
                item.CreatedAt = DateTime.Now;
                item.UpdatedAt = DateTime.Now;
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

        public async Task<T> GetAsync(long id)
            => await Context.Set<T>().FirstOrDefaultAsync(x => x.Id == id);

        public async Task<T> UpdateAsync(T item)
        {
            var oldObj = Context.Set<T>().FirstOrDefault(x => item.Id == x.Id);
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