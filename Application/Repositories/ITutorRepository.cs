namespace Application.Repositories;

public interface ITutorRepository : IRepository<Tutor>
{
    Task<Tutor?> GetByIdWithQualificationsAsync(Guid tutorId, CancellationToken cancellationToken);

    Task<Tutor?> GetByIdWithQualificationsAndAvailabilitiesAsync(Guid tutorId, CancellationToken cancellationToken);

    Task<IEnumerable<Guid>> GetQualificationIdsAsync(Guid tutorId, CancellationToken cancellationToken);
}