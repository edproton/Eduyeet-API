// using System.ComponentModel.DataAnnotations;
// using System.Text;
// using System.Text.Encodings.Web;
// using Microsoft.AspNetCore.Authentication.BearerToken;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.WebUtilities;
// using Microsoft.Extensions.Options;
//
// namespace API.Controllers;
//
// [ApiController]
// [Route("api/[controller]")]
// public class IdentityApiController<TUser>(
//     UserManager<TUser> userManager,
//     SignInManager<TUser> signInManager,
//     IEmailSender<TUser> emailSender,
//     IOptionsMonitor<BearerTokenOptions> bearerTokenOptions,
//     TimeProvider timeProvider,
//     LinkGenerator linkGenerator)
//     : ControllerBase
//     where TUser : class, new()
// {
//     private static readonly EmailAddressAttribute EmailAddressAttribute = new();
//
//     [HttpPost("register")]
//     public async Task<IActionResult> Register([FromBody] RegisterRequest registration)
//     {
//         if (!userManager.SupportsUserEmail)
//         {
//             return Problem("User store doesn't support email.");
//         }
//
//         var email = registration.Email;
//         if (string.IsNullOrEmpty(email) || !EmailAddressAttribute.IsValid(email))
//         {
//             return ValidationProblem(
//                 new ValidationProblemDetails(CreateValidationProblem(userManager.ErrorDescriber.InvalidEmail(email)))
//             );
//         }
//
//         var user = new TUser();
//         await userManager.SetUserNameAsync(user, email);
//         await userManager.SetEmailAsync(user, email);
//         var result = await userManager.CreateAsync(user, registration.Password);
//
//         if (!result.Succeeded)
//         {
//             return ValidationProblem(
//                 new ValidationProblemDetails(CreateValidationProblem(result))
//             );
//         }
//
//         await SendConfirmationEmailAsync(user, email);
//         return Ok();
//     }
//
//     [HttpPost("login")]
//     public async Task<IActionResult> Login(
//         [FromBody] LoginRequest login,
//         [FromQuery] bool? useCookies,
//         [FromQuery] bool? useSessionCookies)
//     {
//         var useCookieScheme = (useCookies == true) || (useSessionCookies == true);
//         var isPersistent = (useCookies == true) && (useSessionCookies != true);
//         signInManager.AuthenticationScheme
//             = useCookieScheme ? IdentityConstants.ApplicationScheme : IdentityConstants.BearerScheme;
//
//         var result = await signInManager.PasswordSignInAsync(login.Email,
//             login.Password,
//             isPersistent,
//             lockoutOnFailure: true);
//
//         if (result.RequiresTwoFactor)
//         {
//             if (!string.IsNullOrEmpty(login.TwoFactorCode))
//             {
//                 result = await signInManager.TwoFactorAuthenticatorSignInAsync(login.TwoFactorCode,
//                     isPersistent,
//                     rememberClient: isPersistent);
//             }
//             else if (!string.IsNullOrEmpty(login.TwoFactorRecoveryCode))
//             {
//                 result = await signInManager.TwoFactorRecoveryCodeSignInAsync(login.TwoFactorRecoveryCode);
//             }
//         }
//
//         if (!result.Succeeded)
//         {
//             return Problem(result.ToString(), statusCode: StatusCodes.Status401Unauthorized);
//         }
//
//         return Ok();
//     }
//
//     [HttpPost("refresh")]
//     public async Task<IActionResult> Refresh([FromBody] RefreshRequest refreshRequest)
//     {
//         var refreshTokenProtector = bearerTokenOptions.Get(IdentityConstants.BearerScheme).RefreshTokenProtector;
//         var refreshTicket = refreshTokenProtector.Unprotect(refreshRequest.RefreshToken);
//
//         if (refreshTicket?.Properties?.ExpiresUtc is not { } expiresUtc ||
//             timeProvider.GetUtcNow() >= expiresUtc ||
//             await signInManager.ValidateSecurityStampAsync(refreshTicket.Principal) is not TUser user)
//         {
//             return Challenge();
//         }
//
//         var newPrincipal = await signInManager.CreateUserPrincipalAsync(user);
//         return SignIn(newPrincipal, IdentityConstants.BearerScheme);
//     }
//
//     [HttpGet("confirmEmail")]
//     public async Task<IActionResult> ConfirmEmail(
//         [FromQuery] string userId,
//         [FromQuery] string code,
//         [FromQuery] string? changedEmail)
//     {
//         var user = await userManager.FindByIdAsync(userId);
//         if (user == null)
//         {
//             return Unauthorized();
//         }
//
//         try
//         {
//             code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
//         }
//         catch (FormatException)
//         {
//             return Unauthorized();
//         }
//
//         IdentityResult result;
//
//         if (string.IsNullOrEmpty(changedEmail))
//         {
//             result = await userManager.ConfirmEmailAsync(user, code);
//         }
//         else
//         {
//             result = await userManager.ChangeEmailAsync(user, changedEmail, code);
//
//             if (result.Succeeded)
//             {
//                 result = await userManager.SetUserNameAsync(user, changedEmail);
//             }
//         }
//
//         if (!result.Succeeded)
//         {
//             return Unauthorized();
//         }
//
//         return Content("Thank you for confirming your email.");
//     }
//
//     [HttpPost("resendConfirmationEmail")]
//     public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationEmailRequest resendRequest)
//     {
//         var user = await userManager.FindByEmailAsync(resendRequest.Email);
//         if (user != null)
//         {
//             await SendConfirmationEmailAsync(user, resendRequest.Email);
//         }
//
//         return Ok();
//     }
//
//     [HttpPost("forgotPassword")]
//     public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest resetRequest)
//     {
//         var user = await userManager.FindByEmailAsync(resetRequest.Email);
//
//         if (user != null && await userManager.IsEmailConfirmedAsync(user))
//         {
//             var code = await userManager.GeneratePasswordResetTokenAsync(user);
//             code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
//
//             await emailSender.SendPasswordResetCodeAsync(user, resetRequest.Email, HtmlEncoder.Default.Encode(code));
//         }
//
//         return Ok();
//     }
//
//     [HttpPost("resetPassword")]
//     public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest resetRequest)
//     {
//         var user = await userManager.FindByEmailAsync(resetRequest.Email);
//
//         if (user == null || !(await userManager.IsEmailConfirmedAsync(user)))
//         {
//             return ValidationProblem(
//                 new ValidationProblemDetails(CreateValidationProblem(userManager.ErrorDescriber.InvalidToken()))
//             );
//         }
//
//         IdentityResult result;
//         try
//         {
//             var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetRequest.ResetCode));
//             result = await userManager.ResetPasswordAsync(user, code, resetRequest.NewPassword);
//         }
//         catch (FormatException)
//         {
//             result = IdentityResult.Failed(userManager.ErrorDescriber.InvalidToken());
//         }
//
//         if (!result.Succeeded)
//         {
//             return ValidationProblem(
//                 new ValidationProblemDetails(CreateValidationProblem(result))
//             );
//         }
//
//         return Ok();
//     }
//
//     [Authorize]
//     [HttpPost("manage/2fa")]
//     public async Task<IActionResult> ManageTwoFactor([FromBody] TwoFactorRequest tfaRequest)
//     {
//         var user = await userManager.GetUserAsync(User);
//         if (user == null)
//         {
//             return NotFound();
//         }
//
//         if (tfaRequest.Enable == true)
//         {
//             if (tfaRequest.ResetSharedKey)
//             {
//                 return ValidationProblem(
//                     new ValidationProblemDetails(
//                         CreateValidationProblem("CannotResetSharedKeyAndEnable",
//                             "Resetting the 2fa shared key must disable 2fa until a 2fa token based on the new shared key is validated.")
//                     )
//                 );
//             }
//             else if (string.IsNullOrEmpty(tfaRequest.TwoFactorCode))
//             {
//                 return ValidationProblem(
//                     new ValidationProblemDetails(
//                         CreateValidationProblem("RequiresTwoFactor",
//                             "No 2fa token was provided by the request. A valid 2fa token is required to enable 2fa.")
//                     )
//                 );
//             }
//             else if (!await userManager.VerifyTwoFactorTokenAsync(user,
//                          userManager.Options.Tokens.AuthenticatorTokenProvider,
//                          tfaRequest.TwoFactorCode))
//             {
//                 return ValidationProblem(
//                     new ValidationProblemDetails(
//                         CreateValidationProblem("InvalidTwoFactorCode",
//                             "The 2fa token provided by the request was invalid. A valid 2fa token is required to enable 2fa.")
//                     )
//                 );
//             }
//
//             await userManager.SetTwoFactorEnabledAsync(user, true);
//         }
//         else if (tfaRequest.Enable == false || tfaRequest.ResetSharedKey)
//         {
//             await userManager.SetTwoFactorEnabledAsync(user, false);
//         }
//
//         if (tfaRequest.ResetSharedKey)
//         {
//             await userManager.ResetAuthenticatorKeyAsync(user);
//         }
//
//         string[]? recoveryCodes = null;
//         if (tfaRequest.ResetRecoveryCodes ||
//             (tfaRequest.Enable == true && await userManager.CountRecoveryCodesAsync(user) == 0))
//         {
//             var recoveryCodesEnumerable = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
//             recoveryCodes = recoveryCodesEnumerable?.ToArray();
//         }
//
//         if (tfaRequest.ForgetMachine)
//         {
//             await signInManager.ForgetTwoFactorClientAsync();
//         }
//
//         var key = await userManager.GetAuthenticatorKeyAsync(user);
//         if (string.IsNullOrEmpty(key))
//         {
//             await userManager.ResetAuthenticatorKeyAsync(user);
//             key = await userManager.GetAuthenticatorKeyAsync(user);
//
//             if (string.IsNullOrEmpty(key))
//             {
//                 throw new NotSupportedException("The user manager must produce an authenticator key after reset.");
//             }
//         }
//
//         return Ok(new TwoFactorResponse
//         {
//             SharedKey = key,
//             RecoveryCodes = recoveryCodes,
//             RecoveryCodesLeft = recoveryCodes?.Length ?? await userManager.CountRecoveryCodesAsync(user),
//             IsTwoFactorEnabled = await userManager.GetTwoFactorEnabledAsync(user),
//             IsMachineRemembered = await signInManager.IsTwoFactorClientRememberedAsync(user),
//         });
//     }
//
//     [Authorize]
//     [HttpGet("manage/info")]
//     public async Task<IActionResult> GetInfo()
//     {
//         var user = await userManager.GetUserAsync(User);
//         if (user == null)
//         {
//             return NotFound();
//         }
//
//         return Ok(await CreateInfoResponseAsync(user));
//     }
//
//     [Authorize]
//     [HttpPost("manage/info")]
//     public async Task<IActionResult> UpdateInfo([FromBody] InfoRequest infoRequest)
//     {
//         var user = await userManager.GetUserAsync(User);
//         if (user == null)
//         {
//             return NotFound();
//         }
//
//         if (!string.IsNullOrEmpty(infoRequest.NewEmail) && !EmailAddressAttribute.IsValid(infoRequest.NewEmail))
//         {
//             return ValidationProblem(
//                 new ValidationProblemDetails(
//                     CreateValidationProblem(userManager.ErrorDescriber.InvalidEmail(infoRequest.NewEmail))
//                 )
//             );
//         }
//
//         if (!string.IsNullOrEmpty(infoRequest.NewPassword))
//         {
//             if (string.IsNullOrEmpty(infoRequest.OldPassword))
//             {
//                 return ValidationProblem(
//                     new ValidationProblemDetails(
//                         CreateValidationProblem("OldPasswordRequired",
//                             "The old password is required to set a new password. If the old password is forgotten, use /resetPassword.")
//                     )
//                 );
//             }
//
//             var changePasswordResult
//                 = await userManager.ChangePasswordAsync(user, infoRequest.OldPassword, infoRequest.NewPassword);
//             if (!changePasswordResult.Succeeded)
//             {
//                 return ValidationProblem(
//                     new ValidationProblemDetails(
//                         CreateValidationProblem(changePasswordResult)
//                     )
//                 );
//             }
//         }
//
//         if (!string.IsNullOrEmpty(infoRequest.NewEmail))
//         {
//             var email = await userManager.GetEmailAsync(user);
//
//             if (email != infoRequest.NewEmail)
//             {
//                 await SendConfirmationEmailAsync(user, infoRequest.NewEmail, isChange: true);
//             }
//         }
//
//         return Ok(await CreateInfoResponseAsync(user));
//     }
//
//     private async Task SendConfirmationEmailAsync(
//         TUser user,
//         string email,
//         bool isChange = false)
//     {
//         var code = isChange
//             ? await userManager.GenerateChangeEmailTokenAsync(user, email)
//             : await userManager.GenerateEmailConfirmationTokenAsync(user);
//         code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
//
//         var userId = await userManager.GetUserIdAsync(user);
//         var routeValues = new RouteValueDictionary()
//         {
//             ["userId"] = userId,
//             ["code"] = code,
//         };
//
//         if (isChange)
//         {
//             routeValues.Add("changedEmail", email);
//         }
//
//         var confirmEmailUrl = linkGenerator.GetUriByAction(HttpContext,
//                                   action: nameof(ConfirmEmail),
//                                   controller: "IdentityApi",
//                                   values: routeValues)
//                               ?? throw new NotSupportedException("Could not generate confirmation email URL.");
//
//         await emailSender.SendConfirmationLinkAsync(user, email, HtmlEncoder.Default.Encode(confirmEmailUrl));
//     }
//
//     private Dictionary<string, string[]> CreateValidationProblem(string errorCode, string errorDescription)
//     {
//         return new Dictionary<string, string[]>
//         {
//             { errorCode, new[] { errorDescription } }
//         };
//     }
//
//
//     private Dictionary<string, string[]> CreateValidationProblem(IdentityError error)
//     {
//         return new Dictionary<string, string[]>
//         {
//             { error.Code, new[] { error.Description } }
//         };
//     }
//
//     private Dictionary<string, string[]> CreateValidationProblem(IdentityResult result)
//     {
//         var errorDictionary = new Dictionary<string, string[]>();
//
//         foreach (var error in result.Errors)
//         {
//             if (errorDictionary.TryGetValue(error.Code, out var descriptions))
//             {
//                 var newDescriptions = new string[descriptions.Length + 1];
//                 Array.Copy(descriptions, newDescriptions, descriptions.Length);
//                 newDescriptions[descriptions.Length] = error.Description;
//                 errorDictionary[error.Code] = newDescriptions;
//             }
//             else
//             {
//                 errorDictionary[error.Code] = new[] { error.Description };
//             }
//         }
//
//         return errorDictionary;
//     }
//
//     private async Task<InfoResponse> CreateInfoResponseAsync(TUser user)
//     {
//         return new InfoResponse
//         {
//             Email = await userManager.GetEmailAsync(user) ??
//                     throw new NotSupportedException("Users must have an email."),
//             IsEmailConfirmed = await userManager.IsEmailConfirmedAsync(user),
//         };
//     }
// }
//
// // Request and Response models
//
// public class RegisterRequest
// {
//     public required string Email { get; set; }
//     public required string Password { get; set; }
// }
//
// public class LoginRequest
// {
//     public required string Email { get; set; }
//     public required string Password { get; set; }
//     public string? TwoFactorCode { get; set; }
//     public string? TwoFactorRecoveryCode { get; set; }
// }
//
// public class RefreshRequest
// {
//     public required string RefreshToken { get; set; }
// }
//
// public class ResendConfirmationEmailRequest
// {
//     public required string Email { get; set; }
// }
//
// public class ForgotPasswordRequest
// {
//     public required string Email { get; set; }
// }
//
// public class ResetPasswordRequest
// {
//     public required string Email { get; set; }
//     public required string ResetCode { get; set; }
//     public required string NewPassword { get; set; }
// }
//
// public class TwoFactorRequest
// {
//     public bool? Enable { get; set; }
//     public bool ResetSharedKey { get; set; }
//     public bool ResetRecoveryCodes { get; set; }
//     public bool ForgetMachine { get; set; }
//     public string? TwoFactorCode { get; set; }
// }
//
// public class TwoFactorResponse
// {
//     public string SharedKey { get; set; } = string.Empty;
//     public string[]? RecoveryCodes { get; set; }
//     public int RecoveryCodesLeft { get; set; }
//     public bool IsTwoFactorEnabled { get; set; }
//     public bool IsMachineRemembered { get; set; }
// }
//
// public class InfoRequest
// {
//     public string? NewEmail { get; set; }
//     public string? NewPassword { get; set; }
//     public string? OldPassword { get; set; }
// }
//
// public class InfoResponse
// {
//     public required string Email { get; set; }
//     public bool IsEmailConfirmed { get; set; }
// }
//
