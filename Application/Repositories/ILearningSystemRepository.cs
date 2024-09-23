namespace Application.Repositories;

public interface ILearningSystemRepository : IRepository<LearningSystem>
{
    Task<LearningSystem?> GetByNameAsync(string name, CancellationToken cancellationToken);
    Task<LearningSystem?> GetByIdWithSubjectsAsync(Guid id, CancellationToken cancellationToken);
}