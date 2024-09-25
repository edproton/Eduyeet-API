using Application.Repositories;
using Domain.Entities;
using Infra.Repositories.Shared;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories;

public class TutorRepository(ApplicationDbContext context) : Repository<Tutor>(context), ITutorRepository
{
    public async Task<bool> HasQualificationForSubjectAsync(
        Guid tutorId,
        Guid subjectId,
        CancellationToken cancellationToken)
    {
        return await Context.Tutors
            .Where(t => t.Id == tutorId)
            .SelectMany(t => t.AvailableQualifications)
            .AnyAsync(q => q.QualificationId == subjectId, cancellationToken);
    }

    public async Task<Tutor?> GetByIdWithQualificationsAsync(Guid tutorId, CancellationToken cancellationToken)
    {
        return await Context.Tutors
            .Include(t => t.AvailableQualifications)
            .FirstOrDefaultAsync(t => t.Id == tutorId, cancellationToken);
    }

    public async Task<Tutor?> GetByIdWithQualificationsAndAvailabilitiesAsync(Guid tutorId, CancellationToken cancellationToken)
    {
        return await Context.Tutors
            .Include(t => t.AvailableQualifications)
            .Include(t => t.Availabilities)
            .FirstOrDefaultAsync(t => t.Id == tutorId, cancellationToken);
    }
}