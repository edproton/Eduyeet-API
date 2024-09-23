namespace Application.Features.UpdateLearningSystem;

public record UpdateLearningSystemCommand(
    Guid Id,
    string Name,
    List<UpdateSubjectCommand> Subjects) : IRequest<ErrorOr<Updated>>;

public record UpdateSubjectCommand(Guid Id, string Name, List<UpdateQualificationCommand> Qualifications);

public record UpdateQualificationCommand(Guid Id, string Name);