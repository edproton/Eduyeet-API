namespace Application.Features.UpdateLearningSystem;

public class UpdateLearningSystemCommandValidator : AbstractValidator<UpdateLearningSystemCommand>
{
    public UpdateLearningSystemCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("System name cannot be empty.")
            .MaximumLength(100).WithMessage("System name cannot exceed 100 characters.");

        RuleFor(c => c.Subjects)
            .NotEmpty().WithMessage("At least one subject is required.");

        RuleForEach(c => c.Subjects).SetValidator(new UpdateSubjectCommandValidator());
    }
}

public class UpdateSubjectCommandValidator : AbstractValidator<UpdateSubjectCommand>
{
    public UpdateSubjectCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Subject name cannot be empty.")
            .MaximumLength(100).WithMessage("Subject name cannot exceed 100 characters.");

        RuleFor(c => c.Qualifications)
            .NotEmpty().WithMessage("At least one qualification is required for each subject.");

        RuleForEach(c => c.Qualifications).SetValidator(new UpdateQualificationCommandValidator());
    }
}

public class UpdateQualificationCommandValidator : AbstractValidator<UpdateQualificationCommand>
{
    public UpdateQualificationCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Qualification name cannot be empty.")
            .MaximumLength(100).WithMessage("Qualification name cannot exceed 100 characters.");
    }
}
