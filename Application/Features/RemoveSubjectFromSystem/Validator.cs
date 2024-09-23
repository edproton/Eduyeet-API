namespace Application.Features.RemoveSubjectFromSystem;

public class RemoveSubjectFromSystemCommandValidator : AbstractValidator<RemoveSubjectFromSystemCommand>
{
    public RemoveSubjectFromSystemCommandValidator()
    {
        RuleFor(c => c.SubjectId)
            .NotEmpty().WithMessage("Subject ID is required.");
    }
}