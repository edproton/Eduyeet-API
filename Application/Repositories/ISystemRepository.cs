namespace Application.Repositories;

public interface ISubjectRepository : IRepository<Subject>
{
    Task<Subject?> GetByIdWithQualificationsAsync(Guid id);

    Task<Subject?> GetByNameAndSystemIdAsync(
        string subjectName,
        Guid learningSystemId,
        CancellationToken cancellationToken);

    Task<IEnumerable<Subject>> GetByLearningSystemIdAsync(Guid systemId, CancellationToken cancellationToken);
    Task<Subject?> GetByNameAsync(string subjectName, CancellationToken cancellationToken);
}