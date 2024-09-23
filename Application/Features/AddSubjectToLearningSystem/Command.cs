namespace Application.Features.AddSubjectToLearningSystem;

public record AddSubjectToSystemCommand(Guid LearningSystemId, string Name) : IRequest<ErrorOr<AddSubjectToSystemCommandResponse>>;
