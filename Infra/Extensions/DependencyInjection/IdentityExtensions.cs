using Application.Services;
using Infra.Options;
using Infra.Repositories.Shared;
using Infra.Services;
using Infra.ValueObjects;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infra.Extensions.DependencyInjection;

public static class IdentityExtensions
{
    public static IServiceCollection AddIdentity(this IServiceCollection services)
    {
        services.AddAuthentication(op =>
            {
                op.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                op.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                op.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                op.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                op.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
                op.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer();
        
        services.ConfigureOptions<JwtOptionsConfigure>();

        services.AddIdentityCore<ApplicationUser>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager<SignInManager<ApplicationUser>>()
            .AddDefaultTokenProviders();

        services
            .AddScoped<IJwtService, JwtService>()
            .AddScoped<IIdentityService, IdentityService>()
            .AddScoped<DevEmailSender>()
            .AddScoped<EmailSender>()
            .AddScoped<IEmailSender<ApplicationUser>>(sp =>
            {
                var environmentOptions = sp.GetRequiredService<IOptions<EnvironmentOptions>>().Value;
                return environmentOptions.IsDevelopment
                    ? sp.GetRequiredService<DevEmailSender>()
                    : sp.GetRequiredService<EmailSender>();
            });

        return services;
    }
}