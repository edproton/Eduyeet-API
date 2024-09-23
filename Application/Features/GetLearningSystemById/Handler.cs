using Application.Features.GetAllLearningSystems;

namespace Application.Features.GetLearningSystemById;

public record GetLearningSystemByIdQuery(Guid Id) : IRequest<ErrorOr<LearningSystemResponse>>;

    public class Handler(ILearningSystemRepository repository)
    : IRequestHandler<GetLearningSystemByIdQuery, ErrorOr<LearningSystemResponse>>
{
    public async Task<ErrorOr<LearningSystemResponse>> Handle(
        GetLearningSystemByIdQuery request,
        CancellationToken cancellationToken)
    {
        var response = await repository.GetByIdWithSubjectsAsync(request.Id, cancellationToken);
        if (response == null)
        {
            return Error.NotFound("LearningSystemNotFound",
                $"A learning system with the id '{request.Id}' was not found.");
        }

        return new LearningSystemResponse(
            response.Id,
            response.Name,
            response.Subjects.Select(subject => new SubjectResponse(
                subject.Id,
                subject.Name,
                subject.Qualifications.Select(qualification => new QualificationResponse(
                    qualification.Id,
                    qualification.Name
                ))
            )));
    }
}