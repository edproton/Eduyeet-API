using Application.Repositories.Shared;
using Domain.Entities;

namespace Application.Repositories;

public interface ISubjectRepository : IRepository<Subject>
{
    Task<Subject?> GetByIdWithQualificationsAsync(Guid id);
}