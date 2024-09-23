using Application.Features.CreatePerson;

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

    Task<ErrorOr<LoginResponse>> LoginAsync(LoginRequest login, CancellationToken httpContextRequestAborted);
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