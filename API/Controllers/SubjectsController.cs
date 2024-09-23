using API.Extensions;
using Application.Features.AddSubjectToLearningSystem;
using Application.Features.RemoveSubjectFromSystem;
using Application.Features.UpdateSubject;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/learning-systems/{learningSystemId}/subjects")]
public class SubjectsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> AddSubjectToSystem(Guid learningSystemId, AddSubjectToSystemCommand command)
    {
        if (learningSystemId != command.LearningSystemId)
        {
            return BadRequest("Inconsistent learning system ID");
        }

        var result = await mediator.Send(command);
        return result.ToHttpActionResult();
    }

    [HttpPut("{subjectId:guid}")]
    public async Task<IActionResult> UpdateSubject(Guid learningSystemId, Guid subjectId, UpdateSubjectCommand command)
    {
        var result = await mediator.Send(command);
        
        return result.ToHttpActionResult();
    }

    [HttpDelete("{subjectId}")]
    public async Task<ActionResult> RemoveSubjectFromSystem(Guid learningSystemId, Guid subjectId)
    {
        var command = new RemoveSubjectFromSystemCommand(subjectId);
        var result = await mediator.Send(command);

        return result.ToHttpActionResult();
    }
}