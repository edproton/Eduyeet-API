namespace Application.Repositories;

public interface ITutorRepository : IRepository<Tutor>
{
    Task<Tutor?> GetByIdWithQualificationsAndAvailabilitiesAsync(Guid personId, CancellationToken cancellationToken);

    Task<List<Tutor>> GetTutorsWithQualificationAsync (Guid qualificationId, CancellationToken cancellationToken);
}