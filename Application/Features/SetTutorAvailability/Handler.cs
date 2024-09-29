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
        RuleFor(c => c.PersonId).NotEmpty().WithMessage("Tutor ID is required.");
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
    IUnitOfWork unitOfWork)
    : IRequestHandler<SetTutorAvailabilityCommand, ErrorOr<SetTutorAvailabilityResponse>>
{
    public async Task<ErrorOr<SetTutorAvailabilityResponse>> Handle(
        SetTutorAvailabilityCommand request,
        CancellationToken cancellationToken)
    {
        var tutor = await tutorRepository.GetByIdWithQualificationsAndAvailabilitiesAsync(request.PersonId, cancellationToken);
        if (tutor == null)
        {
            return Error.NotFound("TutorNotFound", $"A tutor with the ID '{request.PersonId}' was not found.");
        }

        if (!tutor.AvailableQualifications.Any())
        {
            return Error.Validation("NoQualifications", "The tutor must have at least one qualification before setting availability.");
        }

        foreach (var availabilityDto in request.Availabilities)
        {
            var existingAvailability = tutor.Availabilities.FirstOrDefault(a => a.Day == availabilityDto.Day);

            if (existingAvailability != null)
            {
                existingAvailability.TimeSlots = availabilityDto.TimeSlots.Select(ts => new TimeSlot
                {
                    StartTime = ts.StartTime,
                    EndTime = ts.EndTime
                }).ToList();
                await availabilityRepository.UpdateAsync(existingAvailability, cancellationToken);
            }
            else
            {
                var newAvailability = new Availability
                {
                    TutorId = request.PersonId,
                    Day = availabilityDto.Day,
                    TimeSlots = availabilityDto.TimeSlots.Select(ts => new TimeSlot
                    {
                        StartTime = ts.StartTime,
                        EndTime = ts.EndTime
                    }).ToList(),
                    Tutor = tutor
                };
                await availabilityRepository.AddAsync(newAvailability, cancellationToken);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new SetTutorAvailabilityResponse(request.PersonId, request.Availabilities);
    }
}

public record SetTutorAvailabilityResponse(Guid TutorId, List<AvailabilityDto> Availabilities);