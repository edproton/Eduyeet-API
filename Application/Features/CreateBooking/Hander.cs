using Application.Services;

namespace Application.Features.CreateBooking;

public record CreateBookingCommand(
    Guid StudentId,
    Guid TutorId,
    Guid QualificationId,
    DateTime StartTime) : IRequest<ErrorOr<CreateBookingResponse>>;

public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator()
    {
        RuleFor(c => c.StudentId).NotEmpty().WithMessage("Student ID is required.");
        RuleFor(c => c.TutorId).NotEmpty().WithMessage("Tutor ID is required.");
        RuleFor(c => c.QualificationId).NotEmpty().WithMessage("Qualification ID is required.");
        RuleFor(c => c.StartTime)
            .NotEmpty().WithMessage("Start time is required.")
            .GreaterThan(DateTime.UtcNow).WithMessage("Start time must be in the future.");
    }
}

public class CreateBookingHandler(
    IStudentRepository studentRepository,
    ITutorRepository tutorRepository,
    IQualificationRepository qualificationRepository,
    IBookingRepository bookingRepository,
    IUnitOfWork unitOfWork,
    TimeZoneService timeZoneService)
    : IRequestHandler<CreateBookingCommand, ErrorOr<CreateBookingResponse>>
{
    public async Task<ErrorOr<CreateBookingResponse>> Handle(
        CreateBookingCommand request,
        CancellationToken cancellationToken)
    {
        var utcStartTime = request.StartTime.ToUniversalTime();
        var utcEndTime = utcStartTime.AddHours(1);

        var validationResult = await ValidateBookingRequest(request, utcStartTime, utcEndTime, cancellationToken);
        if (validationResult.IsError)
        {
            return validationResult.Errors;
        }

        var (student, tutor, qualification) = validationResult.Value;

        var booking = new Booking
        {
            StudentId = request.StudentId,
            Student = student,
            TutorId = request.TutorId,
            Tutor = tutor,
            QualificationId = request.QualificationId,
            Qualification = qualification,
            StartTime = utcStartTime,
            EndTime = utcEndTime
        };

        await bookingRepository.AddAsync(booking, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateBookingResponse(booking.Id, booking.StudentId, booking.TutorId, booking.QualificationId, booking.StartTime, booking.EndTime);
    }

    private async Task<ErrorOr<(Student, Tutor, Qualification)>> ValidateBookingRequest(
        CreateBookingCommand request,
        DateTime utcStartTime,
        DateTime utcEndTime,
        CancellationToken cancellationToken)
    {
        var student = await studentRepository.GetByIdWithQualificationsAsync(request.StudentId, cancellationToken);
        if (student is null)
        {
            return Errors.Student.NotFound(request.StudentId);
        }

        var tutor = await tutorRepository.GetByIdWithQualificationsAndAvailabilitiesAsync(request.TutorId, cancellationToken);
        if (tutor is null)
        {
            return Errors.Tutor.NotFound(request.TutorId);
        }

        var qualification = await qualificationRepository.GetByIdAsync(request.QualificationId, cancellationToken);
        if (qualification is null)
        {
            return Errors.Qualification.NotFound(request.QualificationId);
        }

        if (!tutor.AvailableQualifications.Any(q => q.Id == request.QualificationId))
        {
            return Errors.Booking.QualificationNotAvailable;
        }

        if (!student.InterestedQualifications.Any(q => q.Id == request.QualificationId))
        {
            return Errors.Booking.StudentNotInterestedInQualification;
        }

        var availability = tutor.Availabilities.FirstOrDefault(a => a.Day == utcStartTime.DayOfWeek);
        if (availability is null)
        {
            return Errors.Booking.TutorNotAvailable;
        }

        var relevantTimeSlot = availability.TimeSlots.FirstOrDefault(ts => 
            ts.ContainsRange(utcStartTime.TimeOfDay, utcEndTime.TimeOfDay));
        if (relevantTimeSlot is null)
        {
            return Errors.Booking.TutorNotAvailable;
        }

        var existingBooking = await bookingRepository.GetOverlappingBookingAsync(request.TutorId, utcStartTime, utcEndTime, cancellationToken);
        if (existingBooking is not null)
        {
            return Errors.Booking.OverlappingBooking;
        }

        return (student, tutor, qualification);
    }
}

public record CreateBookingResponse(Guid BookingId, Guid StudentId, Guid TutorId, Guid QualificationId, DateTime StartTime, DateTime EndTime);