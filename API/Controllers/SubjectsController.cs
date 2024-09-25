using API.Models;
using Application.Features.AddSubjectToLearningSystem;
using Application.Features.GetSubjectsByLearningSystemId;
using Application.Features.RemoveSubjectFromSystem;
using Application.Features.UpdateSubject;

namespace API.Controllers;

[ApiController]
[Route("api/learning-systems/{learningSystemId}/subjects")]
public class SubjectsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddSubjectToSystem(Guid learningSystemId, AddSubjectToSystemCommand command)
    {
        if (learningSystemId != command.LearningSystemId)
        {
            return BadRequest("Inconsistent learning system ID");
        }

        var result = await mediator.Send(command);
        return result.ToHttpActionResult();
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(GetSubjectsByLearningSystemIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSubjectsByLearningSystemId(Guid learningSystemId)
    {
        var query = new GetSubjectsByLearningSystemIdQuery(learningSystemId);
        var result = await mediator.Send(query);

        return result.ToHttpActionResult();
    }

    [HttpPut("{subjectId:guid}")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateSubject(Guid learningSystemId, Guid subjectId, UpdateSubjectCommand command)
    {
        if (subjectId != command.Id)
        {
            return BadRequest("Inconsistent subject ID");
        }

        var result = await mediator.Send(command);
        
        return result.ToHttpActionResult();
    }

    [HttpDelete("{subjectId}")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> RemoveSubjectFromSystem(Guid learningSystemId, Guid subjectId)
    {
        var command = new RemoveSubjectFromSystemCommand(subjectId);
        var result = await mediator.Send(command);

        return result.ToHttpActionResult();
    }
}