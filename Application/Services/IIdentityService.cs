using System.Security.Claims;
using Application.Features.CreatePerson;
using Domain.Enums;

namespace Application.Services;

public interface IIdentityService
{
    Task<ErrorOr<Created>> RegisterUserAsync(
        Guid personId,
        CreatePersonCommand request,
        CancellationToken cancellationToken);
    
    Task<ErrorOr<Success>> ConfirmEmailAsync(
        string userId,
        string code,
        string? changedEmail);

    Task<ErrorOr<LoginResponse>> LoginAsync(LoginRequest login, CancellationToken cancellationToken);

    Task<ErrorOr<GetMeResponse>> GetMeAsync(ClaimsPrincipal user, CancellationToken cancellationToken);
}

public class LoginRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    
    public string? TwoFactorCode { get; set; }

    public string? TwoFactorRecoveryCode { get; set; }
}

public record LoginResponse(
    string Token,
    string RefreshToken);
    
public record GetMeResponse(
    string Id,
    Guid PersonId,
    string Email,
    string Name,
    PersonTypeEnum Type,
    bool EmailConfirmed,
    IEnumerable<Guid> Qualifications);