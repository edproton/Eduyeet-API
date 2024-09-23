namespace Application.Features.RemoveQualificationFromSubject;

public record RemoveQualificationFromSubjectCommand(Guid QualificationId) : IRequest<ErrorOr<Deleted>>;