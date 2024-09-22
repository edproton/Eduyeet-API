using Application.Shared;
using MediatR;

namespace Application.Features.CreateLearningSystem;

public record CreateNewSystemCommand(string Name, List<CreateSubjectCommand> Subjects) : IRequest<Result>;

public record CreateSubjectCommand(string Name, List<CreateQualificationCommand> Qualifications);

public record CreateQualificationCommand(string Name);