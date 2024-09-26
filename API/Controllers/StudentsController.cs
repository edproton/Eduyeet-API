using Application.Features.GetStudentWithQualifications;
using Application.Features.SetStudentQualifications;

namespace API.Controllers;

[ApiController]
[Route("api/students")]
public class StudentsController(ISender mediator) : ControllerBase
{
    [HttpPost("{personId:guid}/qualifications")]
    public async Task<ActionResult> SetStudentQualifications(Guid personId, SetStudentQualificationsCommand command)
    {
        if (personId != command.PersonId)
        {
            return BadRequest("The student ID in the URL does not match the ID in the command.");
        }

        var result = await mediator.Send(command);

        return result.ToHttpActionResult();
    }

    [HttpGet("{studentId:guid}")]
    public async Task<ActionResult> GetStudentWithQualifications(Guid studentId)
    {
        var query = new GetStudentWithQualificationsQuery(studentId);
        var result = await mediator.Send(query);

        return result.ToHttpActionResult();
    }
}