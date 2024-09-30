using Application.Services;
using NodaTime;

namespace Application.Features.SetTutorAvailability;

public record SetTutorAvailabilityCommand(
    Guid PersonId,
    List<AvailabilityDto> Availabilities) : IRequest<ErrorOr<SetTutorAvailabilityResponse>>;

public record AvailabilityDto(DayOfWeek Day, List<TimeSlotDto> TimeSlots);

public record TimeSlotDto(TimeSpan StartTime, TimeSpan EndTime);

public class SetTutorAvailabilityCommandValidator : AbstractValidator<SetTutorAvailabilityCommand>
{
    public SetTutorAvailabilityCommandValidator()
    {
        RuleFor(c => c.PersonId).NotEmpty().WithMessage("Person ID is required.");
        RuleFor(c => c.Availabilities).NotEmpty().WithMessage("At least one availability is required.");
        RuleForEach(c => c.Availabilities).SetValidator(new AvailabilityDtoValidator());
    }
}

public class AvailabilityDtoValidator : AbstractValidator<AvailabilityDto>
{
    public AvailabilityDtoValidator()
    {
        RuleFor(a => a.TimeSlots).NotEmpty().WithMessage("At least one time slot is required.");
        RuleForEach(a => a.TimeSlots).SetValidator(new TimeSlotDtoValidator());
    }
}

public class TimeSlotDtoValidator : AbstractValidator<TimeSlotDto>
{
    public TimeSlotDtoValidator()
    {
        RuleFor(t => t.StartTime).LessThan(t => t.EndTime).WithMessage("Start time must be before end time.");
    }
}

public class SetTutorAvailabilityHandler(
    ITutorRepository tutorRepository,
    IAvailabilityRepository availabilityRepository,
    IUnitOfWork unitOfWork,
    IDateTimeZoneProvider dateTimeZoneProvider,
    TimeZoneService timeZoneService)
    : IRequestHandler<SetTutorAvailabilityCommand, ErrorOr<SetTutorAvailabilityResponse>>
{
    public async Task<ErrorOr<SetTutorAvailabilityResponse>> Handle(
        SetTutorAvailabilityCommand request,
        CancellationToken cancellationToken)
    {
        var tutor = await tutorRepository.GetByIdWithQualificationsAndAvailabilitiesAsync(request.PersonId,
            cancellationToken);
        if (tutor == null)
        {
            return Errors.Tutor.NotFound(request.PersonId);
        }

        if (tutor.AvailableQualifications.Count == 0)
        {
            return Errors.Tutor.NoQualifications;
        }

        foreach (var availabilityDto in request.Availabilities)
        {
            var existingAvailability = tutor.Availabilities.FirstOrDefault(a => a.Day == availabilityDto.Day);
            var utcTimeSlots = ConvertTimeSlotsToUtc(availabilityDto.TimeSlots, tutor.TimeZoneId, availabilityDto.Day);

            if (existingAvailability != null)
            {
                existingAvailability.TimeSlots = utcTimeSlots;
                await availabilityRepository.UpdateAsync(existingAvailability, cancellationToken);
            }
            else
            {
                var newAvailability = new Availability
                {
                    TutorId = request.PersonId,
                    Day = availabilityDto.Day,
                    TimeSlots = utcTimeSlots,
                    Tutor = tutor
                };

                await availabilityRepository.AddAsync(newAvailability, cancellationToken);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new SetTutorAvailabilityResponse(request.PersonId, request.Availabilities);
    }

    private List<TimeSlot> ConvertTimeSlotsToUtc(
        List<TimeSlotDto> timeSlots,
        string tutorTimezoneId,
        DayOfWeek dayOfWeek)
    {
        var zone = dateTimeZoneProvider[tutorTimezoneId];
        var now = SystemClock.Instance.GetCurrentInstant();
        var todayInZone = now.InZone(zone).Date;

        // Find the next occurrence of the target day of the week
        var targetDayOfWeek = (IsoDayOfWeek)((int)dayOfWeek + 1); // Convert DayOfWeek to IsoDayOfWeek
        var targetDate = FindNextOccurrence(todayInZone, targetDayOfWeek);

        return timeSlots.Select(ts =>
        {
            // Create a LocalDateTime in the tutor's time zone on the target day
            var localDateTime = targetDate + LocalTime.FromHourMinuteSecondTick(
                ts.StartTime.Hours, ts.StartTime.Minutes, ts.StartTime.Seconds, ts.StartTime.Milliseconds * 10000);

            // Convert the LocalDateTime to a ZonedDateTime, handling DST automatically
            var zonedDateTime = zone.AtLeniently(localDateTime);

            // Extract the UTC time from the ZonedDateTime
            var utcDateTime = zonedDateTime.ToDateTimeUtc();

            return new TimeSlot
            {
                StartTime = utcDateTime.TimeOfDay,
                EndTime = timeZoneService.ConvertTimeToUtc(ts.EndTime, tutorTimezoneId, dayOfWeek) 
            };
        }).ToList();
    }

    private LocalDate FindNextOccurrence(LocalDate startDate, IsoDayOfWeek targetDayOfWeek)
    {
        var targetDate = startDate;
        while (targetDate.DayOfWeek != targetDayOfWeek)
        {
            targetDate = targetDate.PlusDays(1);
        }
        return targetDate;
    }
}

public record SetTutorAvailabilityResponse(Guid TutorId, List<AvailabilityDto> Availabilities);