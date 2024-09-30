namespace Application.Features.FindAvailableTutors;

public record FindAvailableTutorsQuery(
    Guid QualificationId,
    DateTime RequestedDateTime
) : IRequest<ErrorOr<FindAvailableTutorsResponse>>;

public class FindAvailableTutorsQueryValidator : AbstractValidator<FindAvailableTutorsQuery>
{
    public FindAvailableTutorsQueryValidator()
    {
        RuleFor(q => q.QualificationId).NotEmpty().WithMessage("Qualification ID is required.");
        RuleFor(q => q.RequestedDateTime).NotEmpty().WithMessage("Requested date and time is required.")
            .GreaterThan(DateTime.UtcNow).WithMessage("Requested date and time must be in the future.");
    }
}

public class FindAvailableTutorsHandler(
    ITutorRepository tutorRepository,
    IQualificationRepository qualificationRepository,
    IBookingRepository bookingRepository)
    : IRequestHandler<FindAvailableTutorsQuery, ErrorOr<FindAvailableTutorsResponse>>
{
    public async Task<ErrorOr<FindAvailableTutorsResponse>> Handle(
        FindAvailableTutorsQuery request,
        CancellationToken cancellationToken)
    {
        var qualification = await qualificationRepository.GetByIdAsync(request.QualificationId, cancellationToken);
        if (qualification == null)
        {
            return Error.NotFound("QualificationNotFound", $"A qualification with the ID '{request.QualificationId}' was not found.");
        }

        var tutors = await tutorRepository.GetTutorsWithQualificationAsync(request.QualificationId, cancellationToken);

        var availableTutors = new List<AvailableTutorDto>();
        var utcRequestedDateTime = request.RequestedDateTime.ToUniversalTime();
        var utcEndDateTime = utcRequestedDateTime.AddHours(1);

        foreach (var tutor in tutors)
        {
            var availability = tutor.Availabilities.FirstOrDefault(a => a.Day == utcRequestedDateTime.DayOfWeek);
            if (availability != null)
            {
                var relevantTimeSlot = availability.TimeSlots.FirstOrDefault(ts => 
                    ts.ContainsRange(utcRequestedDateTime.TimeOfDay, utcEndDateTime.TimeOfDay));

                if (relevantTimeSlot != null)
                {
                    // Check if there's no overlapping booking
                    var overlappingBooking = await bookingRepository.GetOverlappingBookingAsync(
                        tutor.Id,
                        utcRequestedDateTime,
                        utcEndDateTime,
                        cancellationToken);

                    if (overlappingBooking == null)
                    {
                        availableTutors.Add(new AvailableTutorDto(
                            tutor.Id,
                            tutor.Name
                        ));
                    }
                }
            }
        }

        return new FindAvailableTutorsResponse(
            request.QualificationId,
            qualification.Name,
            utcRequestedDateTime,
            availableTutors
        );
    }
}

public record FindAvailableTutorsResponse(
    Guid QualificationId,
    string QualificationName,
    DateTime RequestedDateTime,
    List<AvailableTutorDto> AvailableTutors
);

public record AvailableTutorDto(
    Guid Id,
    string Name
);