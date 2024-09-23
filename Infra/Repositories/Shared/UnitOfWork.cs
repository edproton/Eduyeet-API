using Application.Repositories.Shared;

namespace Infra.Repositories.Shared;

public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}