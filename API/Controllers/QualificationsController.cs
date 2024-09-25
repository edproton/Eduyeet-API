using API.Models;
using Application.Features.AddQualificationToSubject;
using Application.Features.GetQualificationsBySubjectId;
using Application.Features.RemoveQualificationFromSubject;

namespace API.Controllers;

[ApiController]
[Route("api/subjects/{subjectId}/qualifications")]
public class QualificationsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddQualificationToSubject(Guid subjectId, AddQualificationToSubjectCommand command)
    {
        if (subjectId != command.SubjectId)
        {
            return BadRequest("Inconsistent subject ID");
        }

        var result = await mediator.Send(command);

        return result.ToHttpActionResult();
    }

    [HttpGet]
    [ProducesResponseType(typeof(GetQualificationsBySubjectIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetQualificationsBySubjectId(Guid subjectId)
    {
        var query = new GetQualificationsBySubjectIdQuery(subjectId);
        var result = await mediator.Send(query);

        return result.ToHttpActionResult();
    }
    
    [HttpDelete("{qualificationId}")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveQualificationFromSubject(Guid subjectId, Guid qualificationId)
    {
        var command = new RemoveQualificationFromSubjectCommand(qualificationId);
        var result = await mediator.Send(command);

        return result.ToHttpActionResult();
    }
}