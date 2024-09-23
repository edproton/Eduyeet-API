namespace Application.Features.CreateLearningSystem;

public record CreateLearningSystemCommand(string Name, List<CreateSubjectCommand> Subjects) : IRequest<ErrorOr<CreateLearningSystemCommandResponse>>;

public record CreateSubjectCommand(string Name, List<CreateQualificationCommand> Qualifications);

public record CreateQualificationCommand(string Name);