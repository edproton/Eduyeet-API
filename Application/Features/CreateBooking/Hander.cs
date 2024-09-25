namespace Application.Features.CreateBooking;

public record CreateBookingCommand(
    Guid StudentId,
    Guid TutorId,
    Guid QualificationId,
    DateTime StartTime) : IRequest<ErrorOr<CreateBookingCommandResponse>>;

public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator()
    {
        RuleFor(c => c.StudentId)
            .NotEmpty().WithMessage("Student ID is required.");

        RuleFor(c => c.TutorId)
            .NotEmpty().WithMessage("Tutor ID is required.");

        RuleFor(c => c.QualificationId)
            .NotEmpty().WithMessage("Qualification ID is required.");

        RuleFor(c => c.StartTime)
            .NotEmpty().WithMessage("Start time is required.")
            .Must(BeInFuture).WithMessage("Start time must be in the future.");
    }

    private static bool BeInFuture(DateTime startTime)
    {
        return startTime > DateTime.UtcNow;
    }
}

public class Handler(
    IUnitOfWork unitOfWork,
    IStudentRepository studentRepository,
    ITutorRepository tutorRepository,
    IQualificationRepository qualificationRepository,
    IBookingRepository bookingRepository)
    : IRequestHandler<CreateBookingCommand, ErrorOr<CreateBookingCommandResponse>>
{
    public async Task<ErrorOr<CreateBookingCommandResponse>> Handle(
        CreateBookingCommand request,
        CancellationToken cancellationToken)
    {
        var (studentResult, tutorResult, qualificationResult) = await GetEntities(request, cancellationToken);

        if (studentResult.IsError)
            return studentResult.Errors;
        if (tutorResult.IsError)
            return tutorResult.Errors;
        if (qualificationResult.IsError)
            return qualificationResult.Errors;

        var student = studentResult.Value;
        var tutor = tutorResult.Value;
        var qualification = qualificationResult.Value;

        var validationResult = await ValidateTutorQualificationsAndAvailability(tutor, qualification, request.StartTime, cancellationToken);
        if (validationResult.IsError)
            return validationResult.Errors;

        var booking = new Booking
        {
            StudentId = student.Id,
            Student = student,
            TutorId = tutor.Id,
            Tutor = tutor,
            QualificationId = qualification.Id,
            Qualification = qualification,
            StartTime = request.StartTime,
            EndTime = request.StartTime.AddMinutes(55)
        };

        await bookingRepository.AddAsync(booking, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateBookingCommandResponse(booking.Id);
    }

    private async Task<(ErrorOr<Student>, ErrorOr<Tutor>, ErrorOr<Qualification>)> GetEntities(
        CreateBookingCommand request,
        CancellationToken cancellationToken)
    {
        var studentTask = studentRepository.GetByIdAsync(request.StudentId, cancellationToken);
        var tutorTask = tutorRepository.GetByIdWithQualificationsAndAvailabilitiesAsync(request.TutorId, cancellationToken);
        var qualificationTask = qualificationRepository.GetByIdAsync(request.QualificationId, cancellationToken);

        await Task.WhenAll(studentTask, tutorTask, qualificationTask);

        ErrorOr<Student> studentResult = studentTask.Result != null
            ? studentTask.Result
            : Error.NotFound("StudentNotFound", $"A student with the ID '{request.StudentId}' was not found.");

        ErrorOr<Tutor> tutorResult = tutorTask.Result != null
            ? tutorTask.Result
            : Error.NotFound("TutorNotFound", $"A tutor with the ID '{request.TutorId}' was not found.");

        ErrorOr<Qualification> qualificationResult = qualificationTask.Result != null
            ? qualificationTask.Result
            : Error.NotFound("QualificationNotFound", $"A qualification with the ID '{request.QualificationId}' was not found.");

        return (studentResult, tutorResult, qualificationResult);
    }

    private async Task<ErrorOr<Success>> ValidateTutorQualificationsAndAvailability(
        Tutor tutor,
        Qualification qualification,
        DateTime startTime,
        CancellationToken cancellationToken)
    {
        if (!tutor.AvailableQualifications.Any())
        {
            return Error.Validation("TutorNotQualified", "The tutor does not have any qualifications.");
        }

        if (!tutor.Availabilities.Any())
        {
            return Error.Validation("TutorNotAvailable", "The tutor does not have any availabilities set.");
        }

        if (tutor.AvailableQualifications.All(q => q.QualificationId != qualification.Id))
        {
            return Error.Validation("TutorNotQualifiedForQualification",
                $"The tutor is not qualified for the qualification with ID '{qualification.Id}'.");
        }

        var endTime = startTime.AddMinutes(55);
        if (!tutor.Availabilities.Any(a => a.IsAvailableAt(startTime) && a.IsAvailableAt(endTime.AddMinutes(-1))))
        {
            return Error.Conflict("TutorNotAvailableAtRequestedTime",
                "The tutor is not available at the requested time.");
        }

        var existingBooking = await bookingRepository.GetOverlappingBookingAsync(tutor.Id, startTime, endTime, cancellationToken);
        if (existingBooking != null)
        {
            return Error.Conflict("OverlappingBooking",
                "The tutor already has a booking during the requested time.");
        }

        return Result.Success;
    }
}

public record CreateBookingCommandResponse(Guid BookingId);