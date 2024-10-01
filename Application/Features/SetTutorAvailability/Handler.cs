using Application.Services;

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
    TimeZoneService timeZoneService)
    : IRequestHandler<SetTutorAvailabilityCommand, ErrorOr<SetTutorAvailabilityResponse>>
{
    public async Task<ErrorOr<SetTutorAvailabilityResponse>> Handle(
        SetTutorAvailabilityCommand request,
        CancellationToken cancellationToken)
    {
        var tutor = await tutorRepository.GetByIdWithQualificationsAndAvailabilitiesAsync(
            request.PersonId,
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

        // Convert the stored UTC availabilities back to the tutor's local time zone
        var localAvailabilities = ConvertAvailabilitiesToLocal(tutor.Availabilities, tutor.TimeZoneId);

        return new SetTutorAvailabilityResponse(request.PersonId, localAvailabilities);
    }

    private List<TimeSlot> ConvertTimeSlotsToUtc(
        List<TimeSlotDto> timeSlots,
        string tutorTimezoneId,
        DayOfWeek dayOfWeek)
    {
        return timeSlots.Select(ts => new TimeSlot
        {
            StartTime = timeZoneService.ConvertTimeToUtc(ts.StartTime, tutorTimezoneId, dayOfWeek),
            EndTime = timeZoneService.ConvertTimeToUtc(ts.EndTime, tutorTimezoneId, dayOfWeek)
        }).ToList();
    }

    private List<AvailabilityDto> ConvertAvailabilitiesToLocal(List<Availability> utcAvailabilities, string tutorTimezoneId)
    {
        return utcAvailabilities.Select(a => new AvailabilityDto(
            a.Day,
            a.TimeSlots.Select(ts => new TimeSlotDto(
                timeZoneService.ConvertTimeFromUtc(ts.StartTime, tutorTimezoneId, a.Day),
                timeZoneService.ConvertTimeFromUtc(ts.EndTime, tutorTimezoneId, a.Day)
            )).ToList()
        )).ToList();
    }
}

public record SetTutorAvailabilityResponse(Guid TutorId, List<AvailabilityDto> Availabilities);
