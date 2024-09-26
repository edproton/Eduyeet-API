using Application.Repositories;
using Domain.Entities;
using Infra.Repositories.Shared;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories;

public class TutorRepository(ApplicationDbContext context) : Repository<Tutor>(context), ITutorRepository
{
    public async Task<Tutor?> GetByIdWithQualificationsAsync(Guid tutorId, CancellationToken cancellationToken)
    {
        return await Context.Tutors
            .Include(t => t.AvailableQualifications)
            .FirstOrDefaultAsync(t => t.Id == tutorId, cancellationToken);
    }

    public async Task<IEnumerable<Guid>> GetQualificationIdsAsync(Guid tutorId, CancellationToken cancellationToken)
    {
        var tutor = await Context.Tutors
            .Where(t => t.Id == tutorId)
            .Select(t => new { t.AvailableQualificationsIds })
            .FirstOrDefaultAsync(cancellationToken);

        return tutor?.AvailableQualificationsIds ?? Enumerable.Empty<Guid>();
    }

    public async Task<Tutor?> GetByIdWithQualificationsAndAvailabilitiesAsync(
        Guid tutorId,
        CancellationToken cancellationToken)
    {
        return await Context.Tutors
            .Include(t => t.AvailableQualifications)
            .Include(t => t.Availabilities)
            .FirstOrDefaultAsync(t => t.Id == tutorId, cancellationToken);
    }
}