using API.Extensions;
using Application.Features.AddQualificationToSubject;
using Application.Features.RemoveQualificationFromSubject;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/subjects/{subjectId}/qualifications")]
public class QualificationsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> AddQualificationToSubject(Guid subjectId, AddQualificationToSubjectCommand command)
    {
        if (subjectId != command.SubjectId)
        {
            return BadRequest("Inconsistent subject ID");
        }

        var result = await mediator.Send(command);

        return result.ToHttpActionResult();
    }

    [HttpDelete("{qualificationId}")]
    public async Task<IActionResult> RemoveQualificationFromSubject(Guid subjectId, Guid qualificationId)
    {
        var command = new RemoveQualificationFromSubjectCommand(qualificationId);
        var result = await mediator.Send(command);

        return result.ToHttpActionResult();
    }
}