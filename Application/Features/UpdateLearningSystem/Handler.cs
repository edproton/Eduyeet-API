namespace Application.Features.UpdateLearningSystem;

public record UpdateLearningSystemCommand(
    Guid Id,
    string Name) : IRequest<ErrorOr<Updated>>;

public class UpdateLearningSystemCommandValidator : AbstractValidator<UpdateLearningSystemCommand>
{
    public UpdateLearningSystemCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("System name cannot be empty.")
            .MaximumLength(100).WithMessage("System name cannot exceed 100 characters.");
    }
}

public class Handler(
    IUnitOfWork unitOfWork,
    ILearningSystemRepository systemRepository)
    : IRequestHandler<UpdateLearningSystemCommand, ErrorOr<Updated>>
{
    public async Task<ErrorOr<Updated>> Handle(
        UpdateLearningSystemCommand request,
        CancellationToken cancellationToken)
    {
        var existingSystem = await systemRepository.GetByIdAsync(request.Id, cancellationToken);
        if (existingSystem == null)
        {
            return Error.NotFound("LearningSystemNotFound",
                $"A learning system with the ID '{request.Id}' was not found.");
        }

        var systemWithSameName = await systemRepository.GetByNameAsync(request.Name, cancellationToken);
        if (systemWithSameName != null && systemWithSameName.Id != request.Id)
        {
            return Error.Conflict("LearningSystemNameAlreadyExists",
                $"A learning system with the name '{request.Name}' already exists.");
        }

        existingSystem.Name = request.Name;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Updated;
    }
}