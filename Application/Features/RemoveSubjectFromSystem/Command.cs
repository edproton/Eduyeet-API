namespace Application.Features.RemoveSubjectFromSystem;

public record RemoveSubjectFromSystemCommand(Guid SubjectId) : IRequest<ErrorOr<Deleted>>;