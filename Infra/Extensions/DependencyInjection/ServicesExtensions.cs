using Application.Services;
using Infra.Options;
using Infra.Services;
using Infra.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infra.Extensions.DependencyInjection;

public static class ServicesExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services
            .AddScoped<IJwtService, JwtService>()
            .AddScoped<IIdentityService, IdentityService>()
            .AddScoped<EmailSender>()
            .AddScoped<DevEmailSender>()
            .AddScoped<IEmailSender<ApplicationUser>>(sp =>
            {
                var environmentOptions = sp.GetRequiredService<IOptions<EnvironmentOptions>>().Value;

                return environmentOptions.EnvironmentType switch
                {
                    EnvironmentType.Development => sp.GetRequiredService<DevEmailSender>(),
                    EnvironmentType.Staging => sp.GetRequiredService<EmailSender>(),
                    EnvironmentType.Production => sp.GetRequiredService<EmailSender>(),
                    _ => throw new ArgumentOutOfRangeException(nameof(environmentOptions.EnvironmentType),
                        $"Not expected environment type: {environmentOptions.Type}")
                };
            });

        return services;
    }
}