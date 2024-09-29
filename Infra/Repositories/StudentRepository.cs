using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infra.Repositories.Shared;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories;

public class StudentRepository(ApplicationDbContext context) : Repository<Student>(context), IStudentRepository
{
    public async Task<IEnumerable<Guid>> GetQualificationIdsAsync(Guid studentId, CancellationToken cancellationToken)
    {
        return await Context.Students
            .Where(t => t.Id == studentId)
            .SelectMany(t => t.InterestedQualifications.Select(q => q.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<Student?> GetByIdWithQualificationsAsync(Guid studentId, CancellationToken cancellationToken)
    {
        var person = await Context.Persons
            .FirstOrDefaultAsync(p => p.Id == studentId, cancellationToken);

        if (person is not { Type: PersonTypeEnum.Student })
        {
            return null;
        }

        var tutor = await Context.Students
            .Include(t => t.InterestedQualifications)
            .FirstOrDefaultAsync(t => t.Id == studentId, cancellationToken);
        
        if (tutor is { InterestedQualifications: null })
        {
            tutor.InterestedQualifications ??= [];
        }

        return tutor;
    }

}

public class PersonRepository(ApplicationDbContext context) : Repository<Person>(context), IPersonRepository;