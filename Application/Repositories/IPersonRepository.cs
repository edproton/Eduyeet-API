namespace Application.Repositories;

public interface IPersonRepository : IRepository<Person>
{
    Task<Person?> GetByEmailAsync(string email, CancellationToken cancellationToken);
}

public interface ITutorRepository : IPersonRepository;

public interface IStudentRepository : IPersonRepository;
