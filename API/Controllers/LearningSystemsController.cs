using API.Models;
using Application.Features.CreateLearningSystem;
using Application.Features.DeleteLearningSystem;
using Application.Features.GetAllLearningSystems;
using Application.Features.GetLearningSystemById;
using Application.Features.UpdateLearningSystem;
using Application.Repositories.Shared;

namespace API.Controllers;

[ApiController]
[Route("api/learning-systems")]
public class LearningSystemsController(ISender mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> CreateLearningSystem(CreateLearningSystemCommand command)
    {
        var result = await mediator.Send(command);

        return result.ToHttpActionResult();
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<LearningSystemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllLearningSystems([FromQuery] int skip = 0, [FromQuery] int take = 10)
    {
        var query = new GetAllLearningSystemsQuery(skip, take);
        var result = await mediator.Send(query);
        return result.ToHttpActionResult();
    }
    
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LearningSystemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLearningSystem(Guid id)
    {
        var query = new GetLearningSystemByIdQuery(id);
        var result = await mediator.Send(query);

        return result.ToHttpActionResult();
    }
    
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(LearningSystemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateLearningSystem(Guid id, UpdateLearningSystemCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("Inconsistent learning system ID");
        }

        var result = await mediator.Send(command);

        return result.ToHttpActionResult();
    }
    
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteLearningSystem(Guid id)
    {
        var command = new DeleteLearningSystemCommand(id);
        var result = await mediator.Send(command);

        return result.ToHttpActionResult();
    }
}