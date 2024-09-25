namespace Application.Features.GetTutorWithQualificationsAndAvailability;

public record GetTutorWithQualificationsAndAvailabilityQuery(Guid TutorId) 
    : IRequest<ErrorOr<GetTutorWithQualificationsAndAvailabilityResponse>>;

public class GetTutorWithQualificationsAndAvailabilityQueryValidator 
    : AbstractValidator<GetTutorWithQualificationsAndAvailabilityQuery>
{
    public GetTutorWithQualificationsAndAvailabilityQueryValidator()
    {
        RuleFor(q => q.TutorId)
            .NotEmpty().WithMessage("Tutor ID is required.");
    }
}

public class GetTutorWithQualificationsAndAvailabilityHandler(
    ITutorRepository tutorRepository)
    : IRequestHandler<GetTutorWithQualificationsAndAvailabilityQuery, 
        ErrorOr<GetTutorWithQualificationsAndAvailabilityResponse>>
{
    public async Task<ErrorOr<GetTutorWithQualificationsAndAvailabilityResponse>> Handle(
        GetTutorWithQualificationsAndAvailabilityQuery request,
        CancellationToken cancellationToken)
    {
        var tutor = await tutorRepository.GetByIdWithQualificationsAndAvailabilitiesAsync(request.TutorId, cancellationToken);
        if (tutor == null)
        {
            return Error.NotFound("TutorNotFound", $"A tutor with the ID '{request.TutorId}' was not found.");
        }
        
        if (!tutor.AvailableQualifications.Any())
        {
            return Error.Validation("TutorHasNoQualifications", "The tutor must have at least one qualification before setting availability.");
        }

        var availabilities = tutor.Availabilities
            .Select(a => new AvailabilityDto(
                a.Day,
                a.TimeSlots.Select(ts => new TimeSlotDto(ts.StartTime, ts.EndTime)).ToList()))
            .ToList();

        var qualifications = tutor.AvailableQualifications
            .Select(q => new QualificationDto(q.QualificationId, q.Subject.Name))
            .ToList();

        return new GetTutorWithQualificationsAndAvailabilityResponse(
            tutor.Id,
            tutor.Name,
            qualifications,
            availabilities);
    }
}

public record GetTutorWithQualificationsAndAvailabilityResponse(
    Guid TutorId,
    string TutorName,
    List<QualificationDto> Qualifications,
    List<AvailabilityDto> Availabilities);

public record AvailabilityDto(DayOfWeek Day, List<TimeSlotDto> TimeSlots);

public record TimeSlotDto(TimeSpan StartTime, TimeSpan EndTime);

public record QualificationDto(Guid SubjectId, string SubjectName);