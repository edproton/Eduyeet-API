namespace Application.Features.AddQualificationToSubject;

public record AddQualificationToSubjectCommand(Guid SubjectId, string Name) : IRequest<ErrorOr<AddQualificationToSubjectCommandResponse>>;
