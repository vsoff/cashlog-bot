using System.Linq.Expressions;
using Cashlog.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cashlog.Data.UoW;

public class Repository<T> : IRepository<T> where T : Entity
{
    private readonly Expression<Func<T, bool>> _defaultExpression = x => true;

    protected readonly ApplicationContext Context;

    public Repository(ApplicationContext context)
    {
        Context = context;
    }

    public Task<T> GetAsync(long id)
    {
        return Context.Set<T>().FirstOrDefaultAsync(x => x.Id == id);
    }

    public Task<T> GetAsync(Expression<Func<T, bool>>? whereExpression = null)
    {
        return Context.Set<T>().FirstOrDefaultAsync(whereExpression ?? _defaultExpression);
    }

    public Task<T[]> GetListAsync(long[] ids)
    {
        return Context.Set<T>().Where(x => ids.Contains(x.Id)).ToArrayAsync();
    }

    public async Task<ICollection<T>> GetListAsync(Expression<Func<T, bool>>? whereExpression = null)
    {
        return await Context.Set<T>().Where(whereExpression ?? _defaultExpression).ToListAsync();
    }

    [Obsolete("Можно использовать GetListAsync(null) вместо этого метода")]
    public Task<T[]> GetAllAsync()
    {
        return Context.Set<T>().ToArrayAsync();
    }

    public Task<bool> AnyAsync(Expression<Func<T, bool>>? whereExpression = null)
    {
        return Context.Set<T>().AnyAsync(whereExpression ?? _defaultExpression);
    }

    public async Task<T> AddAsync(T item)
    {
        return (await AddRangeAsync(new[] { item })).First();
    }

    public async Task<T[]> AddRangeAsync(IEnumerable<T> items)
    {
        var newItems = new List<T>();

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
            throw new InvalidOperationException("Невозможно обновить объект, которого нету в БД");

        Context.Entry(oldObj).State = EntityState.Detached;
        item.CreatedAt = oldObj.CreatedAt;
        item.UpdatedAt = DateTime.Now;
        Context.Entry(item).State = EntityState.Modified;
        return item;
    }

    public Task<T[]> GetListAsync(PartitionRequest partitionRequest, Expression<Func<T, bool>>? whereExpression = null)
    {
        return Context.Set<T>().Where(whereExpression ?? _defaultExpression).Skip(partitionRequest.Skip)
            .Take(partitionRequest.Take).ToArrayAsync();
    }
}