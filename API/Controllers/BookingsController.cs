using Application.Features.CreateBooking;
using Application.Features.FindAvailableTutorsHandler;
using Application.Features.GetAllTutorsByQualificationId;
using Application.Features.GetStudentBookings;
using Application.Features.GetTutorBookings;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingsController(
    ISender mediator) : ControllerBase
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
    
    [HttpGet("available-tutors-by-qualification")]
    public async Task<ActionResult> FindAvailableTutorsByQualification(
        [FromQuery] Guid qualificationId)
    {
        var query = new GetAllTutorsByQualificationIdQuery(qualificationId);
        var result = await mediator.Send(query);

        return result.ToHttpActionResult();
    }
    
    [HttpGet("find-tutor-availability")]
    [AllowAnonymous]
    public async Task<ActionResult> FindAvailableTutors(
        [FromQuery] Guid tutorId,
        [FromQuery] int month,
        [FromQuery] int day,
        [FromQuery] int year,
        [FromQuery] string timeZoneId)
    {
        var query = new FindTutorAvailabilityQuery(tutorId, month, day, year, timeZoneId);
        var result = await mediator.Send(query);

        return result.ToHttpActionResult();
    }
}