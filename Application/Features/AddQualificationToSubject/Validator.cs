namespace Application.Features.AddQualificationToSubject;

public class AddQualificationToSubjectCommandValidator : AbstractValidator<AddQualificationToSubjectCommand>
{
    public AddQualificationToSubjectCommandValidator()
    {
        RuleFor(c => c.SubjectId)
            .NotEmpty().WithMessage("Subject ID is required.");

        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Qualification name cannot be empty.")
            .MaximumLength(100).WithMessage("Qualification name cannot exceed 100 characters.");
    }
}