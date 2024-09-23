namespace Application.Features.CreateLearningSystem;

public record CreateLearningSystemCommand(string Name) : IRequest<ErrorOr<CreateLearningSystemCommandResponse>>;
