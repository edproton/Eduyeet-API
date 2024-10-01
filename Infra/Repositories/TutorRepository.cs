using Application.Repositories;
using Domain.Entities;
using Infra.Repositories.Shared;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories;

public class TutorRepository(ApplicationDbContext context) : Repository<Tutor>(context), ITutorRepository
{
    public async Task<Tutor?> GetByIdWithQualificationsAndAvailabilitiesAsync(Guid id, CancellationToken cancellationToken)
    {
        return await Context.Tutors
            .Include(t => t.AvailableQualifications)
            .Include(t => t.Availabilities)
            .ThenInclude(a => a.TimeSlots)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<List<Tutor>> GetTutorsWithQualificationAndAvailabilitiesAsync(Guid qualificationId, CancellationToken cancellationToken)
    {
        return await Context.Tutors
            .Include(t => t.AvailableQualifications)
            .Include(t => t.Availabilities)
            .ThenInclude(a => a.TimeSlots)
            .Where(t => t.AvailableQualifications.Any(q => q.Id == qualificationId))
            .ToListAsync(cancellationToken);
    }
}