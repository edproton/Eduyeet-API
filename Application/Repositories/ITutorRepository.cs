namespace Application.Repositories;

public interface ITutorRepository : IRepository<Tutor>
{
    Task<Tutor?> GetByIdWithQualificationsAndAvailabilitiesAsync(Guid tutorId, CancellationToken cancellationToken);
    
    Task<Tutor?> GetByIdWithQualificationsAsync(Guid tutorId, CancellationToken cancellationToken);
}