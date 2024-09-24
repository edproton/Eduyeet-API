using Application.Repositories;
using Application.Repositories.Shared;
using Domain.Entities;
using Infra.Repositories.Shared;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories;

public class LearningSystemRepository(ApplicationDbContext context)
    : Repository<LearningSystem>(context), ILearningSystemRepository
{
    public async Task<LearningSystem?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        return await Context.LearningSystems
            .FirstOrDefaultAsync(ls => EF.Functions.ILike(ls.Name, name), cancellationToken);
    }
    
    public async Task<LearningSystem?> GetByIdWithSubjectsAsync(Guid id, CancellationToken cancellationToken)
    {
        return await Context.LearningSystems
            .Include(ls => ls.Subjects)
            .ThenInclude(s => s.Qualifications)
            .FirstOrDefaultAsync(ls => ls.Id == id, cancellationToken);
    }
    
    public override async Task<PaginatedResponse<LearningSystem>> GetAllAsync(
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var query = Context.LearningSystems
            .Skip(skip)
            .Take(take)
            .Include(ls => ls.Subjects)
            .ThenInclude(s => s.Qualifications);

        var totalCount = await Context.LearningSystems.CountAsync(cancellationToken: cancellationToken);
        
        return new PaginatedResponse<LearningSystem>(
            await query.ToListAsync(cancellationToken),
            totalCount
        );
    }
}

public class PersonRepository(ApplicationDbContext context) : Repository<Person>(context), IPersonRepository
{
    public async Task<Person?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await Context.Persons
            .FirstOrDefaultAsync(p => EF.Functions.ILike(p.Email, email), cancellationToken);
    }
}

public class TutorRepository(ApplicationDbContext context) : PersonRepository(context), ITutorRepository;

public class StudentRepository(ApplicationDbContext context) : PersonRepository(context), IStudentRepository;