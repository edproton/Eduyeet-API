namespace Application.Features.GetAllLearningSystems;

public class Handler(ILearningSystemRepository repository)
    : IRequestHandler<GetAllLearningSystemsQuery, ErrorOr<PaginatedResponse<LearningSystemResponse>>>
{
    public async Task<ErrorOr<PaginatedResponse<LearningSystemResponse>>> Handle(GetAllLearningSystemsQuery request, CancellationToken cancellationToken)
    {
        var paginatedResponse = await repository.GetAllAsync(request.Skip, request.Take, cancellationToken);
        
        
        var learningSystemDtos = paginatedResponse.Items.Select(system => new LearningSystemResponse(
            system.Id,
            system.Name,
            system.Subjects.Select(subject => new SubjectResponse(
                subject.Id,
                subject.Name,
                subject.Qualifications.Select(qualification => new QualificationResponse(
                    qualification.Id,
                    qualification.Name
                ))
            ))
        ));
        
        return new PaginatedResponse<LearningSystemResponse>(learningSystemDtos, paginatedResponse.TotalCount);
    }
}