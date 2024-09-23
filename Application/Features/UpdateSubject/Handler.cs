namespace Application.Features.UpdateSubject;

public record UpdateSubjectCommand(
    Guid Id,
    string Name) : IRequest<ErrorOr<Updated>>;

public class UpdateSubjectCommandValidator : AbstractValidator<UpdateSubjectCommand>
{
    public UpdateSubjectCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Subject name cannot be empty.")
            .MaximumLength(100).WithMessage("Subject name cannot exceed 100 characters.");
    }
}

public class Handler(
    IUnitOfWork unitOfWork,
    ISubjectRepository subjectRepository)
    : IRequestHandler<UpdateSubjectCommand, ErrorOr<Updated>>
{
    public async Task<ErrorOr<Updated>> Handle(
        UpdateSubjectCommand request,
        CancellationToken cancellationToken)
    {
        var existingSystem = await subjectRepository.GetByIdAsync(request.Id, cancellationToken);
        if (existingSystem == null)
        {
            return Error.NotFound("SubjectNotFound",
                $"A subject with the ID '{request.Id}' was not found.");
        }

        var systemWithSameName = await subjectRepository.GetByNameAsync(request.Name, cancellationToken);
        if (systemWithSameName != null && systemWithSameName.Id != request.Id)
        {
            return Error.Conflict("SubjectNameAlreadyExists",
                $"A subject with the name '{request.Name}' already exists.");
        }

        existingSystem.Name = request.Name;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Updated;
    }
}