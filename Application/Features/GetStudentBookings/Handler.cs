using Application.Services;

namespace Application.Features.GetStudentBookings;

public record GetStudentBookingsQuery(Guid StudentId) : IRequest<ErrorOr<IEnumerable<GetStudentBookingsResponse>>>;

public class GetStudentBookingsQueryValidator : AbstractValidator<GetStudentBookingsQuery>
{
    public GetStudentBookingsQueryValidator()
    {
        RuleFor(q => q.StudentId).NotEmpty().WithMessage("Student ID is required.");
    }
}

public class GetStudentBookingsHandler(
    TimeZoneService timeZoneService,
    IStudentRepository studentRepository,
    IBookingRepository bookingRepository)
    : IRequestHandler<GetStudentBookingsQuery, ErrorOr<IEnumerable<GetStudentBookingsResponse>>>
{
    public async Task<ErrorOr<IEnumerable<GetStudentBookingsResponse>>> Handle(
        GetStudentBookingsQuery request,
        CancellationToken cancellationToken)
    {
        var student = await studentRepository.GetByIdAsync(request.StudentId, cancellationToken);
        if (student == null)
        {
            return Error.NotFound("StudentNotFound", $"A student with the ID '{request.StudentId}' was not found.");
        }

        // Efficiently fetch bookings with related Tutor and Qualification data
        var bookings = await bookingRepository.GetBookingsByStudentIdWithTutorAndQualificationAsync(
            request.StudentId, cancellationToken);

        var bookingDtos = bookings.Select(b => 
            new GetStudentBookingsResponse(
                b.Id,
                b.TutorId,
                b.Tutor!.Name, // Assuming Tutor is included in the fetched data
                b.QualificationId,
                b.Qualification!.Name, // Assuming Qualification is included in the fetched data
                timeZoneService.ConvertFromUtc(b.StartTime, student.TimeZoneId),
                timeZoneService.ConvertFromUtc(b.EndTime, student.TimeZoneId)
            ));

        return bookingDtos.ToList();
    }
}

public record GetStudentBookingsResponse(
    Guid Id,
    Guid TutorId,
    string TutorName,
    Guid QualificationId,
    string QualificationName,
    DateTime StartTime,
    DateTime EndTime);