using Application.Services;

namespace Application.Features.GetTutorBookings;

public record GetTutorBookingsQuery(Guid TutorId) : IRequest<ErrorOr<GetTutorBookingsResponse>>;

public class GetTutorBookingsQueryValidator : AbstractValidator<GetTutorBookingsQuery>
{
    public GetTutorBookingsQueryValidator()
    {
        RuleFor(q => q.TutorId).NotEmpty().WithMessage("Tutor ID is required.");
    }
}

public class GetTutorBookingsHandler(
    ITutorRepository tutorRepository,
    IBookingRepository bookingRepository,
    TimeZoneService timeZoneService)
    : IRequestHandler<GetTutorBookingsQuery, ErrorOr<GetTutorBookingsResponse>>
{
    public async Task<ErrorOr<GetTutorBookingsResponse>> Handle(
        GetTutorBookingsQuery request,
        CancellationToken cancellationToken)
    {
        var tutor = await tutorRepository.GetByIdAsync(request.TutorId, cancellationToken);
        if (tutor == null)
        {
            return Error.NotFound("TutorNotFound", $"A tutor with the ID '{request.TutorId}' was not found.");
        }

        var bookings = await bookingRepository.GetBookingsByTutorIdAsync(request.TutorId, cancellationToken);

        var bookingDtos = bookings.Select(b => new BookingDto(
            b.Id,
            b.StudentId,
            b.Student.Name,
            b.QualificationId,
            b.Qualification.Name,
            timeZoneService.ConvertFromUtc(b.StartTime, tutor.TimeZoneId),
            timeZoneService.ConvertFromUtc(b.EndTime, tutor.TimeZoneId)
        )).ToList();

        return new GetTutorBookingsResponse(request.TutorId, bookingDtos);
    }
}

public record GetTutorBookingsResponse(Guid TutorId, List<BookingDto> Bookings);

public record BookingDto(
    Guid Id,
    Guid StudentId,
    string StudentName,
    Guid QualificationId,
    string QualificationName,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime);