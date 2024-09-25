namespace Application.Repositories;

public interface ISubjectRepository : IRepository<Subject>
{
    Task<Subject?> GetByIdWithQualificationsAsync(Guid id, CancellationToken cancellationToken);

    Task<Subject?> GetByNameAndSystemIdAsync(
        string subjectName,
        Guid learningSystemId,
        CancellationToken cancellationToken);

    Task<IEnumerable<Subject>> GetByLearningSystemIdAsync(Guid systemId, CancellationToken cancellationToken);
    Task<Subject?> GetByNameAsync(string subjectName, CancellationToken cancellationToken);
}

public interface IBookingRepository : IRepository<Booking>
{
    Task<Booking?> GetOverlappingBookingAsync(Guid tutorId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken);
}