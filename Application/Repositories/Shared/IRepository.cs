using Domain.Entities.Shared;

namespace Application.Repositories.Shared;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PaginatedResponse<T>> GetAllAsync(
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task AddAsync(T entity, CancellationToken cancellationToken);
    Task UpdateAsync(T entity, CancellationToken cancellationToken);
    Task RemoveAsync(T entity, CancellationToken cancellationToken);
}

public class PaginatedResponse<T>(IEnumerable<T> items, int totalCount)
{
    public IEnumerable<T> Items { get; } = items;
    public int TotalCount { get; } = totalCount;
}

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}