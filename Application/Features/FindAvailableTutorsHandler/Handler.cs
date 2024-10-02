
using Application.Services;
using Microsoft.Extensions.Logging;

namespace Application.Features.FindAvailableTutorsHandler;

public record FindTutorAvailabilityQuery(
    Guid TutorId,
    int Month,
    int Day,
    int Year,
    string TimeZoneId)
    : IRequest<ErrorOr<FindTutorAvailabilityResponse>>;

public class FindTutorAvailabilityQueryValidator : AbstractValidator<FindTutorAvailabilityQuery>
{
    public FindTutorAvailabilityQueryValidator()
    {
        RuleFor(q => q.TutorId).NotEmpty().WithMessage("Tutor ID is required.");
        RuleFor(q => q.Month).InclusiveBetween(1, 12).WithMessage("Invalid month.");
        RuleFor(q => q.Day).InclusiveBetween(1, 31).WithMessage("Invalid day.");
        RuleFor(q => q.Year).GreaterThan(2000).WithMessage("Invalid year.");
        RuleFor(q => q.TimeZoneId).NotEmpty().WithMessage("Time Zone ID is required.");
    }
}

public class FindTutorAvailabilityHandler(
    ITutorRepository tutorRepository,
    IBookingRepository bookingRepository,
    TimeZoneService timeZoneService)
    : IRequestHandler<FindTutorAvailabilityQuery, ErrorOr<FindTutorAvailabilityResponse>>
{
    public async Task<ErrorOr<FindTutorAvailabilityResponse>> Handle(
        FindTutorAvailabilityQuery request,
        CancellationToken cancellationToken)
    {
        var tutor = await tutorRepository.GetByIdWithAvailabilitiesAsync(request.TutorId, cancellationToken);
        if (tutor == null)
        {
            return Error.NotFound("TutorNotFound", $"A tutor with the ID '{request.TutorId}' was not found.");
        }

        var requestedDate = new DateTimeOffset(request.Year, request.Month, request.Day, 0, 0, 0, TimeSpan.Zero);
        var requestedDateInTutorTimeZone = timeZoneService.ConvertFromUtc(requestedDate.UtcDateTime, tutor.TimeZoneId);
        var requestedDayOfWeek = requestedDateInTutorTimeZone.DayOfWeek;

        var availableTimeSlots = new List<TimeSlotDto>();

        var availability = tutor.Availabilities.FirstOrDefault(a => a.Day == requestedDayOfWeek);
        if (availability != null)
        {
            foreach (var mainTimeSlot in availability.TimeSlots)
            {
                var tutorStartTime = new DateTimeOffset(requestedDateInTutorTimeZone.Year, requestedDateInTutorTimeZone.Month, requestedDateInTutorTimeZone.Day, 
                    mainTimeSlot.StartTime.Hours, mainTimeSlot.StartTime.Minutes, 0, 
                    TimeSpan.Zero).ToUniversalTime();
                
                var tutorEndTime = new DateTimeOffset(requestedDateInTutorTimeZone.Year, requestedDateInTutorTimeZone.Month, requestedDateInTutorTimeZone.Day, 
                    mainTimeSlot.EndTime.Hours, mainTimeSlot.EndTime.Minutes, 0, 
                    TimeSpan.Zero).ToUniversalTime();

                if (tutorEndTime <= tutorStartTime)
                {
                    tutorEndTime = tutorEndTime.AddDays(1);
                }

                var innerTimeSlots = GenerateInnerTimeSlots(tutorStartTime, tutorEndTime);

                foreach (var (innerStartTime, innerEndTime) in innerTimeSlots)
                {
                    var hasOverlappingBooking = await bookingRepository.GetOverlappingBookingAsync(
                        tutor.Id,
                        innerStartTime.UtcDateTime,
                        innerEndTime.UtcDateTime,
                        cancellationToken);

                    if (hasOverlappingBooking == null)
                    {
                        var localStartTime = timeZoneService.ConvertFromUtc(innerStartTime.UtcDateTime, request.TimeZoneId);
                        var localEndTime = timeZoneService.ConvertFromUtc(innerEndTime.UtcDateTime, request.TimeZoneId);

                        if (localStartTime.Date == requestedDate.Date || localEndTime.Date == requestedDate.Date)
                        {
                            availableTimeSlots.Add(new TimeSlotDto(
                                localStartTime.ToString("HH:mm"),
                                localEndTime.ToString("HH:mm")));
                        }
                    }
                }
            }
        }

        return new FindTutorAvailabilityResponse(
            new TutorDto(tutor.Id, tutor.Name),
            new AvailabilityDto(requestedDayOfWeek, availableTimeSlots));
    }

    private List<(DateTimeOffset StartTime, DateTimeOffset EndTime)> GenerateInnerTimeSlots(DateTimeOffset startTime, DateTimeOffset endTime)
    {
        var innerTimeSlots = new List<(DateTimeOffset StartTime, DateTimeOffset EndTime)>();
        var currentStartTime = startTime;

        while (currentStartTime < endTime)
        {
            var currentEndTime = currentStartTime.AddHours(1);
            if (currentEndTime > endTime)
            {
                currentEndTime = endTime;
            }

            innerTimeSlots.Add((currentStartTime, currentEndTime));
            currentStartTime = currentEndTime;
        }

        return innerTimeSlots;
    }
}

public record FindTutorAvailabilityResponse(TutorDto Tutor, AvailabilityDto Availability);

public record TutorDto(Guid Id, string Name);

public record AvailabilityDto(DayOfWeek WeekDay, List<TimeSlotDto> Timeslots);

public record TimeSlotDto(string StartTime, string EndTime);