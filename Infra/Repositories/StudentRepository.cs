using Application.Repositories;
using Domain.Entities;
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
}

public class PersonRepository(ApplicationDbContext context) : Repository<Person>(context), IPersonRepository;