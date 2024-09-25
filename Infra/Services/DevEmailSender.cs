using Infra.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Infra.Services;

public class DevEmailSender(ILogger<DevEmailSender> logger) : IEmailSender<ApplicationUser>
{
    public async Task SendEmailAsync(string email, string subject, string body)
    {
        logger.LogInformation($"[DEV] Email to: {email}, Subject: {subject}, Body: {body}");
        await Task.CompletedTask;
    }

    public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        var subject = "Confirm your account";
        var body = $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.";
        await SendEmailAsync(email, subject, body);
    }

    public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        var subject = "Reset your password";
        var body = $"Please reset your password by <a href='{resetLink}'>clicking here</a>.";
        await SendEmailAsync(email, subject, body);
    }

    public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        var subject = "Your password reset code";
        var body = $"Your password reset code is: <strong>{resetCode}</strong>";
        await SendEmailAsync(email, subject, body);
    }
}