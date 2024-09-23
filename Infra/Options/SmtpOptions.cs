using System.ComponentModel.DataAnnotations;

namespace Infra.Options;

public class SmtpOptions
{
    [Required]
    public required string Host { get; init; }

    [Required]
    public required int Port { get; init; }

    [Required]
    public required string Username { get; init; }

    [Required]
    public required string Password { get; init; }

    [Required]
    public required bool EnableSsl { get; init; }
    
    [Required]
    public required string SenderEmail { get; init; }
}