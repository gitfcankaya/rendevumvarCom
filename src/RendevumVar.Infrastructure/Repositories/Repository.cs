using Microsoft.EntityFrameworkCore;
using RendevumVar.Core.Repositories;
using RendevumVar.Infrastructure.Data;
using System.Linq.Expressions;

namespace RendevumVar.Infrastructure.Repositories;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null)
    {
        return predicate == null
            ? await _dbSet.CountAsync()
            : await _dbSet.CountAsync(predicate);
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities)
    {
        await _dbSet.AddRangeAsync(entities);
        await _context.SaveChangesAsync();
    }

    public virtual async Task UpdateAsync(TEntity entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task UpdateRangeAsync(IEnumerable<TEntity> entities)
    {
        _dbSet.UpdateRange(entities);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(TEntity entity)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteRangeAsync(IEnumerable<TEntity> entities)
    {
        _dbSet.RemoveRange(entities);
        await _context.SaveChangesAsync();
    }

    public virtual async Task SoftDeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            var isDeletedProperty = entity.GetType().GetProperty("IsDeleted");
            var deletedAtProperty = entity.GetType().GetProperty("DeletedAt");

            if (isDeletedProperty != null)
            {
                isDeletedProperty.SetValue(entity, true);
            }

            if (deletedAtProperty != null)
            {
                deletedAtProperty.SetValue(entity, DateTime.UtcNow);
            }

            await UpdateAsync(entity);
        }
    }

    public virtual async Task RestoreAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            var isDeletedProperty = entity.GetType().GetProperty("IsDeleted");
            var deletedAtProperty = entity.GetType().GetProperty("DeletedAt");

            if (isDeletedProperty != null)
            {
                isDeletedProperty.SetValue(entity, false);
            }

            if (deletedAtProperty != null)
            {
                deletedAtProperty.SetValue(entity, null);
            }

            await UpdateAsync(entity);
        }
    }
}
