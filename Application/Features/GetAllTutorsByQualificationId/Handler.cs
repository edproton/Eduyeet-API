using Application.Services;

namespace Application.Features.GetAllTutorsByQualificationId;

public record GetAllTutorsByQualificationIdQuery(
    Guid QualificationId)
    : IRequest<ErrorOr<GetAllTutorsByQualificationIdResponse>>;

public class GetAllTutorsByQualificationIdQueryValidator : AbstractValidator<GetAllTutorsByQualificationIdQuery>
{
    public GetAllTutorsByQualificationIdQueryValidator()
    {
        RuleFor(q => q.QualificationId).NotEmpty().WithMessage("Qualification ID is required.");
    }
}

public class GetAllTutorsByQualificationIdHandler(
    IQualificationRepository qualificationRepository,
    ITutorRepository tutorRepository)
    : IRequestHandler<GetAllTutorsByQualificationIdQuery, ErrorOr<GetAllTutorsByQualificationIdResponse>>
{
    public async Task<ErrorOr<GetAllTutorsByQualificationIdResponse>> Handle(
        GetAllTutorsByQualificationIdQuery request,
        CancellationToken cancellationToken)
    {
        var qualification = await qualificationRepository.GetByIdAsync(request.QualificationId, cancellationToken);
        if (qualification == null)
        {
            return Error.NotFound("QualificationNotFound",
                $"A qualification with the ID '{request.QualificationId}' was not found.");
        }

        var tutors = await tutorRepository.GetTutorsWithQualificationAndAvailabilitiesAsync(request.QualificationId,
            cancellationToken);

        return new GetAllTutorsByQualificationIdResponse(tutors.Select(t => new TutorDto(t.Id, t.Name)));
    }
}

public record GetAllTutorsByQualificationIdResponse(IEnumerable<TutorDto> Tutors);

public record AvailabilityDto(Guid Id, DateTime StartTime, DateTime EndTime);

public record TimeSlotDto(Guid Id, DateTime StartTime, DateTime EndTime);

public record TutorDto(Guid Id, string Name);