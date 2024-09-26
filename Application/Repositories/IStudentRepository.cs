namespace Application.Repositories;

public interface IStudentRepository : IRepository<Student>
{
    Task<IEnumerable<Guid>> GetQualificationIdsAsync(Guid studentId, CancellationToken cancellationToken);
    Task<Student?> GetByIdWithQualificationsAsync(Guid studentId, CancellationToken cancellationToken);
}

public interface IPersonRepository : IRepository<Person>;