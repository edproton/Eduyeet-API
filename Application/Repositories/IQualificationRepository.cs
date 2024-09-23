namespace Application.Repositories;

public interface IQualificationRepository : IRepository<Qualification>
{
    Task<Qualification?> GetByNameAndSubjectIdAsync(
        string qualificationName,
        Guid subjectId,
        CancellationToken cancellationToken);

    Task<IEnumerable<Qualification>> GetBySubjectIdAsync(Guid subjectId, CancellationToken cancellationToken);
}