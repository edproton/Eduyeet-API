namespace Application.Features.GetAllLearningSystems;

public record LearningSystemResponse(Guid Id, string Name, IEnumerable<SubjectResponse> Subjects);

public record SubjectResponse(Guid Id, string Name, IEnumerable<QualificationResponse> Qualifications);

public record QualificationResponse(Guid Id, string Name);