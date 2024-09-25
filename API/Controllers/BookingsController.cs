using Application.Features.CreateBooking;

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
}