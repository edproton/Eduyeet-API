namespace Application.Features.CreateLearningSystem;

public record CreateLearningSystemCommand(string Name) : IRequest<ErrorOr<CreateLearningSystemCommandResponse>>;

public class CreateLearningSystemCommandValidator : AbstractValidator<CreateLearningSystemCommand>
{
    public CreateLearningSystemCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("System name cannot be empty.")
            .MaximumLength(100).WithMessage("System name cannot exceed 100 characters.");
    }
}

public class Handler(
    IUnitOfWork unitOfWork,
    ILearningSystemRepository systemRepository,
    ISubjectRepository subjectRepository,
    IQualificationRepository qualificationRepository)
    : IRequestHandler<CreateLearningSystemCommand, ErrorOr<CreateLearningSystemCommandResponse>>
{
    public async Task<ErrorOr<CreateLearningSystemCommandResponse>> Handle(
        CreateLearningSystemCommand request,
        CancellationToken cancellationToken)
    {
        var existingSystem = await systemRepository.GetByNameAsync(request.Name, cancellationToken);
        if (existingSystem != null)
        {
            return Error.Conflict("LearningSystemAlreadyExists",
                $"A learning system with the name '{request.Name}' already exists.");
        }

        var system = new LearningSystem { Name = request.Name };

        await systemRepository.AddAsync(system, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateLearningSystemCommandResponse(system.Id);
    }
}

public record CreateLearningSystemCommandResponse(Guid LearningSystemId);