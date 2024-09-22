using FluentValidation;

namespace Application.Features.CreateLearningSystem;

public class CreateNewSystemCommandValidator : AbstractValidator<CreateNewSystemCommand>
{
    public CreateNewSystemCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("System name cannot be empty.")
            .MaximumLength(100).WithMessage("System name cannot exceed 100 characters.");

        RuleFor(c => c.Subjects)
            .NotEmpty().WithMessage("At least one subject is required.");

        RuleForEach(c => c.Subjects).SetValidator(new CreateSubjectCommandValidator());
    }
}

public class CreateSubjectCommandValidator : AbstractValidator<CreateSubjectCommand>
{
    public CreateSubjectCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Subject name cannot be empty.")
            .MaximumLength(100).WithMessage("Subject name cannot exceed 100 characters.");

        RuleFor(c => c.Qualifications)
            .NotEmpty().WithMessage("At least one qualification is required for each subject.");

        RuleForEach(c => c.Qualifications).SetValidator(new CreateQualificationCommandValidator());
    }
}

public class CreateQualificationCommandValidator : AbstractValidator<CreateQualificationCommand>
{
    public CreateQualificationCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Qualification name cannot be empty.")
            .MaximumLength(100).WithMessage("Qualification name cannot exceed 100 characters.");
    }
}
