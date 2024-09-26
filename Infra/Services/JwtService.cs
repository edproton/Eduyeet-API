using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infra.Options;
using Infra.ValueObjects;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infra.Services;


public interface IJwtService
{
    Task<string> GenerateToken(ApplicationUser user, CancellationToken cancellationToken);
    ClaimsPrincipal? ValidateToken(string token);
}

public class JwtService(IOptions<JwtOptions> jwtOptions, IUserService userService) : IJwtService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<string> GenerateToken(ApplicationUser user, CancellationToken cancellationToken)
    {
        if (user.UserName == null)
        {
            throw new ArgumentNullException(nameof(user.UserName));
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("type", user.Person.Type.ToString().ToLower()),
            new("personId", user.PersonId.ToString())
        };
        
        var qualificationIds = await userService.GetUserQualificationIds(user.PersonId, user.Person.Type, cancellationToken);
        foreach (var qualificationId in qualificationIds)
        {
            claims.Add(new Claim("qualifications", qualificationId.ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpireInMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtOptions.Secret);
        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtOptions.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return principal;
        }
        catch
        {
            return null!;
        }
    }
}