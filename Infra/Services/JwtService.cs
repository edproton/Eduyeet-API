using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Infra.Options;
using Infra.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infra.Services;


public interface IJwtService
{
    string GenerateToken(ApplicationUser user);
    ClaimsPrincipal? ValidateToken(string token);
}

public class JwtService(IOptions<JwtOptions> jwtOptions) : IJwtService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public string GenerateToken(ApplicationUser user)
    {
        if (user.UserName == null)
        {
            throw new ArgumentNullException(nameof(user.UserName));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

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