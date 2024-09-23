namespace Application.Features.CreateLearningSystem;

public class CreateLearningSystemCommandValidator : AbstractValidator<CreateLearningSystemCommand>
{
    public CreateLearningSystemCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("System name cannot be empty.")
            .MaximumLength(100).WithMessage("System name cannot exceed 100 characters.");
    }
}