namespace Application.Repositories;

public interface IQualificationRepository : IRepository<Qualification>
{
    Task<Qualification?> GetByNameAndSubjectIdAsync(
        string qualificationName,
        Guid subjectId,
        CancellationToken cancellationToken);

    Task<IEnumerable<Qualification>> GetBySubjectIdAsync(Guid subjectId, CancellationToken cancellationToken);

    Task<Qualification?> GetByNameAsync(string qualificationName, CancellationToken cancellationToken);

    Task<List<Qualification>> GetQualificationsByIdsAsync(List<Guid> qualificationIds, CancellationToken cancellationToken);
}