namespace Application.Features.RemoveQualificationFromSubject;

public class RemoveQualificationFromSubjectCommandValidator : AbstractValidator<RemoveQualificationFromSubjectCommand>
{
    public RemoveQualificationFromSubjectCommandValidator()
    {
        RuleFor(c => c.QualificationId)
            .NotEmpty().WithMessage("Qualification ID is required.");
    }
}