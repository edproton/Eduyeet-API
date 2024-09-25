using Application.Repositories;
using Domain.Entities;
using Infra.Repositories.Shared;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories;

public class SubjectRepository(ApplicationDbContext context) : Repository<Subject>(context), ISubjectRepository
{
    public async Task<Subject?> GetByIdWithQualificationsAsync(Guid id, CancellationToken cancellationToken)
    {
        return await Context.Subjects
            .Include(s => s.Qualifications)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Subject?> GetByNameAndSystemIdAsync(
        string subjectName,
        Guid learningSystemId,
        CancellationToken cancellationToken)
    {
        return await Context.Subjects
            .FirstOrDefaultAsync(s =>
                    EF.Functions.ILike(s.Name, subjectName) &&
                    s.LearningSystemId == learningSystemId,
                cancellationToken);
    }

    public async Task<IEnumerable<Subject>> GetByLearningSystemIdAsync(Guid systemId, CancellationToken cancellationToken)
    {
        return await Context.Subjects
            .Where(s => s.LearningSystemId == systemId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Subject?> GetByNameAsync(string subjectName, CancellationToken cancellationToken)
    {
        return await Context.Subjects
            .FirstOrDefaultAsync(s => EF.Functions.ILike(s.Name, subjectName), cancellationToken);
    }
}