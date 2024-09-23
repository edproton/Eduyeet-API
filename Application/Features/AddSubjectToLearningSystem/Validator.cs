namespace Application.Features.AddSubjectToLearningSystem;

public class AddSubjectToSystemCommandValidator : AbstractValidator<AddSubjectToSystemCommand>
{
    public AddSubjectToSystemCommandValidator()
    {
        RuleFor(c => c.LearningSystemId)
            .NotEmpty().WithMessage("Learning system ID is required.");

        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Subject name cannot be empty.")
            .MaximumLength(100).WithMessage("Subject name cannot exceed 100 characters.");
    }
}