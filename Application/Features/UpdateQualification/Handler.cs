namespace Application.Features.UpdateQualification;

public record UpdateQualificationCommand(
    Guid Id,
    string Name) : IRequest<ErrorOr<Updated>>;

public class UpdateQualificationCommandValidator : AbstractValidator<UpdateQualificationCommand>
{
    public UpdateQualificationCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Qualification name cannot be empty.")
            .MaximumLength(100).WithMessage("Qualification name cannot exceed 100 characters.");
    }
}

public class Handler(
    IUnitOfWork unitOfWork,
    IQualificationRepository qualificationRepository)
    : IRequestHandler<UpdateQualificationCommand, ErrorOr<Updated>>
{
    public async Task<ErrorOr<Updated>> Handle(
        UpdateQualificationCommand request,
        CancellationToken cancellationToken)
    {
        var existingSystem = await qualificationRepository.GetByIdAsync(request.Id, cancellationToken);
        if (existingSystem == null)
        {
            return Error.NotFound("QualificationNotFound",
                $"A qualification with the ID '{request.Id}' was not found.");
        }

        var systemWithSameName = await qualificationRepository.GetByNameAsync(request.Name, cancellationToken);
        if (systemWithSameName != null && systemWithSameName.Id != request.Id)
        {
            return Error.Conflict("QualificationNameAlreadyExists",
                $"A qualification with the name '{request.Name}' already exists.");
        }

        existingSystem.Name = request.Name;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Updated;
    }
}