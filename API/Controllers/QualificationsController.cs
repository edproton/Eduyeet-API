using API.Extensions;
using API.Models;
using Application.Features.AddQualificationToSubject;
using Application.Features.GetAllLearningSystems;
using Application.Features.RemoveQualificationFromSubject;
using Application.Features.UpdateQualification;
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

    [HttpPut("{qualificationId:guid}")]
    [ProducesResponseType(typeof(LearningSystemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateQualification(Guid subjectId, Guid qualificationId, UpdateQualificationCommand command)
    {
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