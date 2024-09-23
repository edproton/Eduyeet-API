using Infra.Extensions.DependencyInjection;
using Infra.Options;
using Infra.Repositories;
using Infra.Repositories.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infra;

public static class DependencyInjection
{
    public static IServiceCollection AddInfra(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptionsWithValidation<EnvironmentOptions>()
            .AddOptionsWithValidation<DatabaseOptions>();

        services
            .AddDynamicDbContext<ApplicationDbContext>(configuration)
            .AddRepositories();

        return services;
    }
}


