namespace Application.Features.GetSubjectsByLearningSystemId;

public record GetSubjectsByLearningSystemIdQuery(Guid LearningSystemId) : IRequest<ErrorOr<GetSubjectsByLearningSystemIdResponse>>;

public class GetSubjectsByLearningSystemIdQueryValidator : AbstractValidator<GetSubjectsByLearningSystemIdQuery>
{
    public GetSubjectsByLearningSystemIdQueryValidator()
    {
        RuleFor(q => q.LearningSystemId)
            .NotEmpty().WithMessage("Learning System ID is required.");
    }
}

public class GetSubjectsByLearningSystemIdHandler(ILearningSystemRepository learningSystemRepository)
    : IRequestHandler<GetSubjectsByLearningSystemIdQuery, ErrorOr<GetSubjectsByLearningSystemIdResponse>>
{
    public async Task<ErrorOr<GetSubjectsByLearningSystemIdResponse>> Handle(
        GetSubjectsByLearningSystemIdQuery request,
        CancellationToken cancellationToken)
    {
        var learningSystem = await learningSystemRepository.GetByIdWithSubjectsAsync(request.LearningSystemId, cancellationToken);
        if (learningSystem == null)
        {
            return Error.NotFound("LearningSystemNotFound", $"A learning system with the ID '{request.LearningSystemId}' was not found.");
        }

        var subjects = learningSystem.Subjects
            .Select(s => new SubjectDto(s.Id, s.Name))
            .ToList();

        return new GetSubjectsByLearningSystemIdResponse(
            learningSystem.Id,
            learningSystem.Name,
            subjects);
    }
}

public record GetSubjectsByLearningSystemIdResponse(
    Guid LearningSystemId,
    string LearningSystemName,
    List<SubjectDto> Subjects);

public record SubjectDto(Guid Id, string Name);