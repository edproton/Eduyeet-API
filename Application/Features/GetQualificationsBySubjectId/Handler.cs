namespace Application.Features.GetQualificationsBySubjectId;

public record GetQualificationsBySubjectIdQuery(Guid SubjectId) : IRequest<ErrorOr<GetQualificationsBySubjectIdResponse>>;

public class GetQualificationsBySubjectIdQueryValidator : AbstractValidator<GetQualificationsBySubjectIdQuery>
{
    public GetQualificationsBySubjectIdQueryValidator()
    {
        RuleFor(q => q.SubjectId)
            .NotEmpty().WithMessage("Subject ID is required.");
    }
}

public class GetQualificationsBySubjectIdHandler(ISubjectRepository subjectRepository)
    : IRequestHandler<GetQualificationsBySubjectIdQuery, ErrorOr<GetQualificationsBySubjectIdResponse>>
{
    public async Task<ErrorOr<GetQualificationsBySubjectIdResponse>> Handle(
        GetQualificationsBySubjectIdQuery request,
        CancellationToken cancellationToken)
    {
        var subject = await subjectRepository.GetByIdWithQualificationsAsync(request.SubjectId, cancellationToken);
        if (subject == null)
        {
            return Error.NotFound("SubjectNotFound", $"A subject with the ID '{request.SubjectId}' was not found.");
        }

        var qualifications = subject.Qualifications
            .Select(q => new QualificationDto(q.Id, q.Name))
            .ToList();

        return new GetQualificationsBySubjectIdResponse(
            subject.Id,
            subject.Name,
            qualifications);
    }
}

public record GetQualificationsBySubjectIdResponse(
    Guid SubjectId,
    string SubjectName,
    List<QualificationDto> Qualifications);

public record QualificationDto(Guid Id, string Name);