namespace Application.Features.SetStudentQualifications;

public record SetStudentQualificationsCommand(
    Guid PersonId,
    List<Guid> QualificationIds) : IRequest<ErrorOr<SetStudentQualificationsResponse>>;

public class SetStudentQualificationsCommandValidator : AbstractValidator<SetStudentQualificationsCommand>
{
    public SetStudentQualificationsCommandValidator()
    {
        RuleFor(c => c.PersonId).NotEmpty().WithMessage("Student ID is required.");
        RuleFor(c => c.QualificationIds).NotEmpty().WithMessage("At least one qualification is required.");
    }
}

public class SetStudentQualificationsHandler(
    IStudentRepository studentRepository,
    IQualificationRepository qualificationRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SetStudentQualificationsCommand, ErrorOr<SetStudentQualificationsResponse>>
{
    public async Task<ErrorOr<SetStudentQualificationsResponse>> Handle(
        SetStudentQualificationsCommand request,
        CancellationToken cancellationToken)
    {
        var student = await studentRepository.GetByIdWithQualificationsAsync(request.PersonId, cancellationToken);
        if (student == null)
        {
            return Error.NotFound("StudentNotFound", $"A student with the ID '{request.PersonId}' was not found.");
        }

        var qualifications = await qualificationRepository.GetQualificationsByIdsAsync(request.QualificationIds, cancellationToken);
        if (qualifications.Count != request.QualificationIds.Count)
        {
            return Error.Validation("InvalidQualifications", "One or more qualification IDs are invalid.");
        }

        student.InterestedQualifications = qualifications;

        await studentRepository.UpdateAsync(student, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new SetStudentQualificationsResponse(student.Id, qualifications.Select(q => q.Id).ToList());
    }
}

public record SetStudentQualificationsResponse(Guid StudentId, List<Guid> QualificationIds);
