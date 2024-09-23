namespace Application.Features.AddQualificationToSubject;

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