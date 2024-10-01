using Application.Services;

namespace Application.Features.CreateBooking;

public record CreateBookingCommand(
    Guid StudentId,
    Guid TutorId,
    Guid QualificationId,
    DateTime StartTime) : IRequest<ErrorOr<CreateBookingResponse>>;

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

        var utcStartTime = timeZoneService.ConvertToUtc(request.StartTime, student.TimeZoneId);
        var utcEndTime = utcStartTime.AddHours(1);

        var validationResult = await ValidateBookingRequest(request, student, tutor, utcStartTime, utcEndTime, cancellationToken);
        if (validationResult.IsError)
        {
            return validationResult.Errors;
        }

        var qualification = validationResult.Value;

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

        var studentLocalStartTime = timeZoneService.ConvertFromUtc(booking.StartTime, student.TimeZoneId);
        var studentLocalEndTime = timeZoneService.ConvertFromUtc(booking.EndTime, student.TimeZoneId);
        var tutorLocalStartTime = timeZoneService.ConvertFromUtc(booking.StartTime, tutor.TimeZoneId);
        var tutorLocalEndTime = timeZoneService.ConvertFromUtc(booking.EndTime, tutor.TimeZoneId);

        return new CreateBookingResponse(
            booking.Id, 
            booking.StudentId, 
            booking.TutorId, 
            booking.QualificationId, 
            studentLocalStartTime, 
            studentLocalEndTime,
            tutorLocalStartTime,
            tutorLocalEndTime,
            booking.StartTime,
            booking.EndTime
        );
    }

    private async Task<ErrorOr<Qualification>> ValidateBookingRequest(
        CreateBookingCommand request,
        Student student,
        Tutor tutor,
        DateTime utcStartTime,
        DateTime utcEndTime,
        CancellationToken cancellationToken)
    {
        var qualification = await qualificationRepository.GetByIdAsync(request.QualificationId, cancellationToken);
        if (qualification is null)
        {
            return Errors.Qualification.NotFound(request.QualificationId);
        }

        if (tutor.AvailableQualifications.All(q => q.Id != request.QualificationId))
        {
            return Errors.Booking.QualificationNotAvailable;
        }

        if (student.InterestedQualifications.All(q => q.Id != request.QualificationId))
        {
            return Errors.Booking.StudentNotInterestedInQualification;
        }

        var availability = tutor.Availabilities.FirstOrDefault(a => a.Day == utcStartTime.DayOfWeek);
        if (availability is null)
        {
            return Errors.Booking.TutorNotAvailable;
        }

        var relevantTimeSlot = availability.TimeSlots.FirstOrDefault(ts =>
            ts.StartTime <= utcStartTime.TimeOfDay && ts.EndTime >= utcEndTime.TimeOfDay);

        if (relevantTimeSlot is null)
        {
            return Errors.Booking.TutorNotAvailable;
        }

        var existingBooking = await bookingRepository.GetOverlappingBookingAsync(request.TutorId, utcStartTime, utcEndTime, cancellationToken);
        if (existingBooking is not null)
        {
            return Errors.Booking.OverlappingBooking;
        }

        return qualification;
    }
}

public record CreateBookingResponse(
    Guid BookingId, 
    Guid StudentId, 
    Guid TutorId, 
    Guid QualificationId, 
    DateTimeOffset StudentLocalStartTime, 
    DateTimeOffset StudentLocalEndTime,
    DateTimeOffset TutorLocalStartTime,
    DateTimeOffset TutorLocalEndTime,
    DateTimeOffset UtcStartTime,
    DateTimeOffset UtcEndTime);