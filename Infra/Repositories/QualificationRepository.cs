using Application.Repositories;
using Domain.Entities;
using Infra.Repositories.Shared;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories;

public class QualificationRepository(ApplicationDbContext context)
    : Repository<Qualification>(context), IQualificationRepository
{
    public async Task<Qualification?> GetByNameAndSubjectIdAsync(
        string qualificationName,
        Guid subjectId,
        CancellationToken cancellationToken)
    {
        return await Context.Qualifications
            .FirstOrDefaultAsync(q =>
                    EF.Functions.ILike(q.Name, qualificationName) &&
                    q.QualificationId == subjectId,
                cancellationToken);
    }

    public async Task<IEnumerable<Qualification>> GetBySubjectIdAsync(Guid subjectId, CancellationToken cancellationToken)
    {
        return await Context.Qualifications
            .Where(q => q.QualificationId == subjectId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Qualification?> GetByNameAsync(string qualificationName, CancellationToken cancellationToken)
    {
        return await Context.Qualifications
            .FirstOrDefaultAsync(q => EF.Functions.ILike(q.Name, qualificationName), cancellationToken);
    }

    public async Task<List<Qualification>> GetQualificationsByIdsAsync(List<Guid> qualificationIds, CancellationToken cancellationToken)
    {
        return await Context.Qualifications
            .Where(q => qualificationIds.Contains(q.Id))
            .ToListAsync(cancellationToken);
    }
}