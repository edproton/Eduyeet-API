using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infra.Options;

public class JwtOptions
{
    [Required]
    public required string Secret { get; init; }

    [Required]
    public required string Issuer { get; init; }

    [Required]
    public required string Audience { get; init; }

    [Required]
    public required int ExpireInMinutes { get; init; }
}

public class JwtOptionsConfigure(IOptions<JwtOptions> jwtOptions) : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public void Configure(JwtBearerOptions options)
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret)),
            ValidateIssuer = true,
            ValidIssuer = _jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtOptions.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new()
        {
            OnForbidden = (ctx) =>
            {
                
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = (ctx) =>
            {
                
                return Task.CompletedTask;
            },
            OnChallenge = (ctx) =>
            {
                
                return Task.CompletedTask;
            },
            OnTokenValidated = (ctx) =>
            {
                
                return Task.CompletedTask;
            },
        };
    }

    public void Configure(string? name, JwtBearerOptions options)
    {
        Configure(options);
    }
}