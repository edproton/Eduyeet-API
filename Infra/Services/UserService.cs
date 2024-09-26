using Application.Repositories;
using Domain.Enums;

namespace Infra.Services;

public interface IUserService
{
    Task<IEnumerable<Guid>> GetUserQualificationIds(Guid personId, PersonTypeEnum personType, CancellationToken cancellationToken);
}

public class UserService(IStudentRepository studentRepository, ITutorRepository tutorRepository)
    : IUserService
{
    public async Task<IEnumerable<Guid>> GetUserQualificationIds(Guid personId, PersonTypeEnum personType, CancellationToken cancellationToken)
    {
        return personType switch
        {
            PersonTypeEnum.Student => await studentRepository.GetQualificationIdsAsync(personId, cancellationToken),
            PersonTypeEnum.Tutor => await tutorRepository.GetQualificationIdsAsync(personId, cancellationToken),
            _ => throw new InvalidOperationException($"Invalid person type: {personType}")
        };
    }
}