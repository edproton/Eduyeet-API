namespace Application.Repositories;

public interface ITutorRepository : IRepository<Tutor>
{
    Task<Tutor?> GetByIdWithQualificationsAndAvailabilitiesAsync(Guid personId, CancellationToken cancellationToken);

    Task<List<Tutor>> GetTutorsWithQualificationAndAvailabilitiesAsync (Guid qualificationId, CancellationToken cancellationToken);

    Task<Tutor?> GetByIdWithAvailabilitiesAsync(Guid tutorId, CancellationToken cancellationToken);
}