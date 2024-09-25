using Application.Features.CreatePerson;
using Application.Services;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController(
    ISender mediator,
    IIdentityService identityService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        var result = await identityService.LoginAsync(request, HttpContext.RequestAborted);
        
        return result.ToHttpActionResult();
    }
    
    [HttpPost("register")]
    public async Task<ActionResult<ErrorOr<Created>>> RegisterPersonAsync(
        [FromBody] CreatePersonCommand request)
    {
        var result = await mediator.Send(request, HttpContext.RequestAborted);

        return result.ToHttpActionResult();
    }
    
    [HttpGet("confirmEmail")]
    public async Task<IActionResult> ConfirmEmail(
        [FromQuery] string userId,
        [FromQuery] string code,
        [FromQuery] string? changedEmail)
    {
        var result = await identityService.ConfirmEmailAsync(userId, code, changedEmail);

        return result.ToHttpActionResult();
    }
}