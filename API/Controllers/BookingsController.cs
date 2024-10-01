using Application.Features.CreateBooking;
using Application.Features.FindAvailableTutorsHandler;
using Application.Features.GetStudentBookings;
using Application.Features.GetTutorBookings;

namespace API.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingsController(
    ISender mediator,
    TimeProvider timeProvider) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult> CreateBooking(CreateBookingCommand command)
    {
        var result = await mediator.Send(command);

        return result.ToHttpActionResult();
    }

    [HttpGet("students/{studentId:guid}")]
    public async Task<ActionResult> GetStudentBookings(Guid studentId)
    {
        var query = new GetStudentBookingsQuery(studentId);
        var result = await mediator.Send(query);

        return result.ToHttpActionResult();
    }

    [HttpGet("tutors/{tutorId:guid}")]
    public async Task<ActionResult> GetTutorBookings(Guid tutorId)
    {
        var query = new GetTutorBookingsQuery(tutorId);
        var result = await mediator.Send(query);

        return result.ToHttpActionResult();
    }
    
    [HttpGet("available-tutors")]
    public async Task<ActionResult> FindAvailableTutors(
        [FromQuery] Guid qualificationId, 
        [FromQuery] string timeZoneId)
    {
        var query = new FindAvailableTutorsQuery(qualificationId, timeZoneId);
        var result = await mediator.Send(query);

        return result.ToHttpActionResult();
    }
}