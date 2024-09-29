using Application.Features.CreateBooking;
using Application.Features.FindAvailableTutors;
using Application.Features.GetStudentBookings;
using Application.Features.GetTutorBookings;

namespace API.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingsController(ISender mediator) : ControllerBase
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
    public async Task<ActionResult> FindAvailableTutors([FromQuery] Guid qualificationId, [FromQuery] DateTime requestedDateTime)
    {
        var query = new FindAvailableTutorsQuery(qualificationId, requestedDateTime);
        var result = await mediator.Send(query);

        return result.ToHttpActionResult();
    }
}