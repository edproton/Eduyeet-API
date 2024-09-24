using Application.Services;
using Infra.Options;
using Infra.Repositories.Shared;
using Infra.Services;
using Infra.ValueObjects;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infra.Extensions.DependencyInjection;

public static class IdentityExtensions
{
    public static IServiceCollection AddIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();
        
        services.ConfigureOptions<JwtOptionsConfigure>();

        services.AddIdentityCore<ApplicationUser>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager<SignInManager<ApplicationUser>>()
            .AddDefaultTokenProviders();

        services
            .AddScoped<IJwtService, JwtService>()
            .AddScoped<IIdentityService, IdentityService>()
            .AddScoped<IEmailSender<ApplicationUser>, EmailSender>();

        return services;
    }
}