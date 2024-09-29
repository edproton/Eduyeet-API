using Application.Features.SetTutorAvailability;
using Application.Features.SetTutorQualifications;

namespace API.Controllers;

[ApiController]
[Route("api/tutors")]
public class TutorsController(ISender mediator) : ControllerBase
{
    [HttpPost("{personId:guid}/qualifications")]
    public async Task<ActionResult> SetTutorQualifications(Guid personId, SetTutorQualificationsCommand command)
    {
        if (personId != command.PersonId)
        {
            return BadRequest("The tutor ID in the URL does not match the ID in the command.");
        }

        var result = await mediator.Send(command);

        return result.ToHttpActionResult();
    }

    [HttpPost("{personId:guid}/availability")]
    public async Task<ActionResult> SetTutorAvailability(Guid personId, SetTutorAvailabilityCommand command)
    {
        if (personId != command.PersonId)
        {
            return BadRequest("The tutor ID in the URL does not match the ID in the command.");
        }

        var result = await mediator.Send(command);

        return result.ToHttpActionResult();
    }
}