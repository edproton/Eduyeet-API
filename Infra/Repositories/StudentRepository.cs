using Application.Repositories;
using Domain.Entities;
using Infra.Repositories.Shared;

namespace Infra.Repositories;

public class StudentRepository(ApplicationDbContext context) : Repository<Student>(context), IStudentRepository;

public class PersonRepository(ApplicationDbContext context) : Repository<Person>(context), IPersonRepository;