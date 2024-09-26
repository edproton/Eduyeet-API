using Application.Features.GetTutorWithQualificationsAndAvailability;
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

    [HttpPost("{tutorId:guid}/availability")]
    public async Task<ActionResult> SetTutorAvailability(Guid tutorId, SetTutorAvailabilityCommand command)
    {
        if (tutorId != command.TutorId)
        {
            return BadRequest("The tutor ID in the URL does not match the ID in the command.");
        }

        var result = await mediator.Send(command);

        return result.ToHttpActionResult();
    }

    [HttpGet("{tutorId:guid}")]
    public async Task<ActionResult> GetTutorWithQualificationsAndAvailability(Guid tutorId)
    {
        var query = new GetTutorWithQualificationsAndAvailabilityQuery(tutorId);
        var result = await mediator.Send(query);

        return result.ToHttpActionResult();
    }
}