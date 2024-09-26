using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Application.Features.CreatePerson;
using Application.Repositories;
using Application.Services;
using Domain.Entities;
using ErrorOr;
using Infra.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;

namespace Infra.Services;

public class IdentityService(
    UserManager<ApplicationUser> userManager,
    IEmailSender<ApplicationUser> emailSender,
    LinkGenerator linkGenerator,
    IJwtService jwtService,
    IPersonRepository personRepository,
    ITutorRepository tutorRepository,
    IStudentRepository studentRepository,
    IHttpContextAccessor httpContextAccessor)
    : IIdentityService
{
    public async Task<ErrorOr<Created>> RegisterUserAsync(
        Guid personId,
        CreatePersonCommand command,
        CancellationToken cancellationToken)
    {
        if (!userManager.SupportsUserEmail)
        {
            return Error.Failure("EmailNotSupported", "User store doesn't support email.");
        }

        var userExists = await userManager.FindByEmailAsync(command.Email);
        if (userExists != null)
        {
            return Error.Failure("EmailAlreadyRegistered", "Email already registered.");
        }

        var user = new ApplicationUser { UserName = command.Email, PersonId = personId, Email = command.Email };
        var result = await userManager.CreateAsync(user, command.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => Error.Validation(e.Code, e.Description)).ToList();

            return ErrorOr<Created>.From(errors);
        }

        await SendConfirmationEmailAsync(user, command.Email);

        return Result.Created;
    }

    public async Task<ErrorOr<Success>> ConfirmEmailAsync(
        string userId,
        string code,
        string? changedEmail)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Error.Failure("UserNotFound", "User not found.");
        }

        if (user.EmailConfirmed)
        {
            return Error.Failure("EmailAlreadyConfirmed", "Email already confirmed.");
        }

        try
        {
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        }
        catch (FormatException)
        {
            return Error.Failure("InvalidCode", "Invalid code.");
        }

        IdentityResult result;

        if (string.IsNullOrEmpty(changedEmail))
        {
            result = await userManager.ConfirmEmailAsync(user, code);
        }
        else
        {
            result = await userManager.ChangeEmailAsync(user, changedEmail, code);

            if (result.Succeeded)
            {
                result = await userManager.SetUserNameAsync(user, changedEmail);
            }
        }

        if (!result.Succeeded)
        {
            return Error.Failure("FailedToConfirmEmail", "Failed to confirm email.");
        }

        return Result.Success;
    }

    public async Task<ErrorOr<LoginResponse>> LoginAsync(
        LoginRequest login,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(login.Email);
        if (user == null)
        {
            return Error.NotFound("UserNotFound", "User not found.");
        }

        var result = await userManager.CheckPasswordAsync(user, login.Password);
        if (!result)
        {
            return Error.Failure("LoginFailed", "Invalid password.");
        }

        if (await userManager.GetTwoFactorEnabledAsync(user))
        {
            if (!string.IsNullOrEmpty(login.TwoFactorCode))
            {
                var validVerification = await userManager.VerifyTwoFactorTokenAsync(
                    user,
                    userManager.Options.Tokens.AuthenticatorTokenProvider,
                    login.TwoFactorCode);
                if (!validVerification)
                {
                    return Error.Failure("InvalidTwoFactorCode", "Invalid two-factor code.");
                }
            }
            else if (!string.IsNullOrEmpty(login.TwoFactorRecoveryCode))
            {
                var redeemTwoFactorResult
                    = await userManager.RedeemTwoFactorRecoveryCodeAsync(user, login.TwoFactorRecoveryCode);
                if (!redeemTwoFactorResult.Succeeded)
                {
                    return Error.Failure("InvalidRecoveryCode", "Invalid recovery code.");
                }
            }
            else
            {
                return Error.Failure("TwoFactorRequired", "Two-factor authentication is required.");
            }
        }

        var person = await personRepository.GetByIdAsync(user.PersonId, cancellationToken);
        user.Person = person ?? throw new InvalidOperationException("Person not found.");

        var token = await jwtService.GenerateToken(user, cancellationToken);

        return new LoginResponse(token, "MOCK_REFRESH_TOKEN");
    }

    public async Task<ErrorOr<GetMeResponse>> GetMeAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var existingUser = await userManager.GetUserAsync(user);
        if (existingUser == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        var person = await personRepository.GetByIdAsync(existingUser.PersonId, cancellationToken);
        existingUser.Person = person ?? throw new InvalidOperationException("Person not found.");

        var qualifications = await GetQualificationIds(person, cancellationToken);

        return new GetMeResponse(
            existingUser.Id,
            existingUser.PersonId,
            existingUser.Email!,
            existingUser.Person.Name,
            existingUser.Person.Type,
            existingUser.EmailConfirmed,
            qualifications);
    }

    private async Task SendConfirmationEmailAsync(
        ApplicationUser user,
        string email,
        bool isChange = false)
    {
        var code = isChange
            ? await userManager.GenerateChangeEmailTokenAsync(user, email)
            : await userManager.GenerateEmailConfirmationTokenAsync(user);

        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var userId = await userManager.GetUserIdAsync(user);
        var routeValues = new RouteValueDictionary()
        {
            ["userId"] = userId,
            ["code"] = code,
        };

        var httpContext = httpContextAccessor.HttpContext
                          ?? throw new InvalidOperationException("HttpContext is not available.");

        var confirmEmailUrl = linkGenerator.GetUriByAction(httpContext,
                                  action: "ConfirmEmail",
                                  controller: "Auth",
                                  values: routeValues)
                              ?? throw new InvalidOperationException("Could not generate confirmation email URL.");

        await emailSender.SendConfirmationLinkAsync(user, email, HtmlEncoder.Default.Encode(confirmEmailUrl));
    }

    private async Task<IEnumerable<Guid>> GetQualificationIds(Person person, CancellationToken cancellationToken)
    {
        var qualificationIds = person switch
        {
            Student student => await studentRepository.GetQualificationIdsAsync(student.Id, cancellationToken),
            Tutor tutor => await tutorRepository.GetQualificationIdsAsync(tutor.Id, cancellationToken),
            _ => throw new InvalidOperationException($"Invalid person type: {person.GetType().Name}")
        };

        return qualificationIds ??
               throw new InvalidOperationException($"{person.GetType().Name} not found or has no qualifications.");
    }
}