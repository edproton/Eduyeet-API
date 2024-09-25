namespace Application.Features.AddQualificationToSubject;

public record AddQualificationToSubjectCommand(Guid SubjectId, string Name) : IRequest<ErrorOr<AddQualificationToSubjectCommandResponse>>;

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

public class Handler(
    IUnitOfWork unitOfWork,
    ISubjectRepository subjectRepository,
    IQualificationRepository qualificationRepository)
    : IRequestHandler<AddQualificationToSubjectCommand, ErrorOr<AddQualificationToSubjectCommandResponse>>
{
    public async Task<ErrorOr<AddQualificationToSubjectCommandResponse>> Handle(
        AddQualificationToSubjectCommand request,
        CancellationToken cancellationToken)
    {
        var subject = await subjectRepository.GetByIdAsync(request.SubjectId, cancellationToken);
        if (subject == null)
        {
            return Error.Conflict("SubjectNotFound", $"A subject with the ID '{request.SubjectId}' was not found.");
        }

        var existingQualification = await qualificationRepository.GetByNameAndSubjectIdAsync(
            request.Name,
            request.SubjectId,
            cancellationToken);

        if (existingQualification != null)
        {
            return Error.Conflict("QualificationAlreadyExists",
                $"A qualification with the name '{request.Name}' already exists for the subject with the ID '{request.SubjectId}'.");
        }

        var qualification = new Qualification { Name = request.Name, Subject = subject };
        await qualificationRepository.AddAsync(qualification, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AddQualificationToSubjectCommandResponse(qualification.Id);
    }
}

public record AddQualificationToSubjectCommandResponse(Guid SubjectId);