namespace Application.Features.GetAllLearningSystems;

public record GetAllLearningSystemsQuery(int Skip, int Take) : IRequest<ErrorOr<PaginatedResponse<LearningSystemResponse>>>;

