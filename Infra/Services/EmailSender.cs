using System.Net;
using System.Net.Mail;
using Infra.Options;
using Infra.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infra.Services;

public class EmailSender(ILogger<EmailSender> logger, IOptions<SmtpOptions> smptOptions) : IEmailSender<ApplicationUser>
{
    private readonly SmtpOptions _smtpOptions = smptOptions.Value;
    
    private async Task SendEmailAsync(
        string email,
        string subject,
        string body)
    {
        try
        {
            using (var client = new SmtpClient(_smtpOptions.Host, _smtpOptions.Port))
            {
                client.Credentials = new NetworkCredential(_smtpOptions.Username, _smtpOptions.Password);
                client.EnableSsl = _smtpOptions.EnableSsl;

                var message = new MailMessage
                {
                    From = new MailAddress(_smtpOptions.SenderEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                message.To.Add(email);

                await client.SendMailAsync(message);
                logger.LogInformation($"Email sent successfully to {email}");
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Failed to send email to {email}. Error: {ex.Message}");
            throw;
        }
    }

    public async Task SendConfirmationLinkAsync(
        ApplicationUser user,
        string email,
        string confirmationLink)
    {
        string subject = "Confirm your account";
        string body = $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.";
        await SendEmailAsync(email, subject, body);
    }

    public async Task SendPasswordResetLinkAsync(
        ApplicationUser user,
        string email,
        string resetLink)
    {
        string subject = "Reset your password";
        string body = $"Please reset your password by <a href='{resetLink}'>clicking here</a>.";
        await SendEmailAsync(email, subject, body);
    }

    public async Task SendPasswordResetCodeAsync(
        ApplicationUser user,
        string email,
        string resetCode)
    {
        string subject = "Your password reset code";
        string body = $"Your password reset code is: <strong>{resetCode}</strong>";
        await SendEmailAsync(email, subject, body);
    }
}