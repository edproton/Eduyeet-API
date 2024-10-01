using Application.Services;

namespace Application.Features.FindAvailableTutorsHandler;

public record FindAvailableTutorsQuery(Guid QualificationId, string TimeZoneId) 
    : IRequest<ErrorOr<IEnumerable<FindAvailableTutorsResponse>>>;

public class FindAvailableTutorsQueryValidator : AbstractValidator<FindAvailableTutorsQuery>
{
    public FindAvailableTutorsQueryValidator()
    {
        RuleFor(q => q.QualificationId).NotEmpty().WithMessage("Qualification ID is required.");
        RuleFor(q => q.TimeZoneId).NotEmpty().WithMessage("Time Zone ID is required.");
    }
}

public class FindAvailableTutorsHandler(
    IQualificationRepository qualificationRepository,
    ITutorRepository tutorRepository,
    IBookingRepository bookingRepository,
    TimeZoneService timeZoneService)
    : IRequestHandler<FindAvailableTutorsQuery, ErrorOr<IEnumerable<FindAvailableTutorsResponse>>>
{
    public async Task<ErrorOr<IEnumerable<FindAvailableTutorsResponse>>> Handle(
        FindAvailableTutorsQuery request,
        CancellationToken cancellationToken)
    {
        var qualification = await qualificationRepository.GetByIdAsync(request.QualificationId, cancellationToken);
        if (qualification == null)
        {
            return Error.NotFound("QualificationNotFound", $"A qualification with the ID '{request.QualificationId}' was not found.");
        }

        var tutors = await tutorRepository.GetTutorsWithQualificationAndAvailabilitiesAsync(request.QualificationId, cancellationToken);

       var availableTutors = new List<FindAvailableTutorsResponse>();

        foreach (var tutor in tutors)
        {
            var availableAvailabilities = new List<AvailabilityDto>();

            foreach (var availability in tutor.Availabilities)
            {
                var availableTimeSlots = new List<TimeSlotDto>();

                foreach (var timeSlot in availability.TimeSlots)
                {
                    // Check for overlapping bookings in UTC
                    var hasOverlappingBooking = await bookingRepository.GetOverlappingBookingAsync(
                        tutor.Id,
                        new DateTime(timeSlot.StartTime.Ticks, DateTimeKind.Utc), // Ensure DateTimeKind is UTC
                        new DateTime(timeSlot.EndTime.Ticks, DateTimeKind.Utc),   // Ensure DateTimeKind is UTC
                        cancellationToken);

                    if (hasOverlappingBooking == null)
                    {
                        // Convert to the requested time zone
                        var startTime = timeZoneService.ConvertTimeFromUtc(timeSlot.StartTime, request.TimeZoneId, availability.Day);
                        var endTime = timeZoneService.ConvertTimeFromUtc(timeSlot.EndTime, request.TimeZoneId, availability.Day);

                        availableTimeSlots.Add(new TimeSlotDto(
                            startTime.ToString(@"hh\:mm"),
                            endTime.ToString(@"hh\:mm")));
                    }
                }

                if (availableTimeSlots.Any())
                {
                    availableAvailabilities.Add(new AvailabilityDto(
                        availability.Day,
                        availableTimeSlots));
                }
            }

            if (availableAvailabilities.Any())
            {
                availableTutors.Add(new FindAvailableTutorsResponse(
                    new TutorDto(tutor.Id, tutor.Name),
                    availableAvailabilities));
            }
        }

        return availableTutors;
    }
}

public record FindAvailableTutorsResponse(TutorDto Tutor, List<AvailabilityDto> Availabilities);

public record TutorDto(Guid Id, string Name);

public record AvailabilityDto(DayOfWeek WeekDay, List<TimeSlotDto> Timeslots);

public record TimeSlotDto(string StartTime, string EndTime);