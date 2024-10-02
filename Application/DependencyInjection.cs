using Application.Pipelines;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddScoped<TimeZoneService>();
        services.AddSingleton(TimeProvider.System);

        return services;
    }
}