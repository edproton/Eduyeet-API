namespace Application.Features.GetStudentWithQualifications;

public record GetStudentWithQualificationsQuery(Guid StudentId) 
    : IRequest<ErrorOr<GetStudentWithQualificationsResponse>>;

public class GetStudentWithQualificationsQueryValidator 
    : AbstractValidator<GetStudentWithQualificationsQuery>
{
    public GetStudentWithQualificationsQueryValidator()
    {
        RuleFor(q => q.StudentId)
            .NotEmpty().WithMessage("Student ID is required.");
    }
}

public class GetStudentWithQualificationsHandler(
    IStudentRepository studentRepository)
    : IRequestHandler<GetStudentWithQualificationsQuery, 
        ErrorOr<GetStudentWithQualificationsResponse>>
{
    public async Task<ErrorOr<GetStudentWithQualificationsResponse>> Handle(
        GetStudentWithQualificationsQuery request,
        CancellationToken cancellationToken)
    {
        var student = await studentRepository.GetByIdWithQualificationsAsync(request.StudentId, cancellationToken);
        if (student == null)
        {
            return Error.NotFound("StudentNotFound", $"A student with the ID '{request.StudentId}' was not found.");
        }
        
        if (!student.InterestedQualifications.Any())
        {
            return Error.Validation("StudentHasNoQualifications", "The student has not selected any qualifications.");
        }

        var qualifications = student.InterestedQualifications
            .Select(q => new QualificationDto(q.QualificationId, q.Subject.Name))
            .ToList();

        return new GetStudentWithQualificationsResponse(
            student.Id,
            student.Name,
            qualifications);
    }
}

public record GetStudentWithQualificationsResponse(
    Guid StudentId,
    string StudentName,
    List<QualificationDto> Qualifications);

public record QualificationDto(Guid SubjectId, string SubjectName);