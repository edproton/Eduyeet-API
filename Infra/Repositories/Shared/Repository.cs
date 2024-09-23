using Application.Repositories.Shared;
using Domain.Entities.Shared;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories.Shared;

public class Repository<T>(ApplicationDbContext context) : IRepository<T>
    where T : BaseEntity
{
    protected readonly ApplicationDbContext Context = context;
    protected readonly DbSet<T> DbSet = context.Set<T>();

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<PaginatedResponse<T>> GetAllAsync(
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var query = DbSet.AsQueryable();
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip(skip).Take(take).ToListAsync(cancellationToken);

        return new PaginatedResponse<T>(items, totalCount);
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    public Task UpdateAsync(T entity, CancellationToken cancellationToken)
    {
        Context.Entry(entity).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(T entity, CancellationToken cancellationToken)
    {
        DbSet.Remove(entity);
        return Task.CompletedTask;
    }
}