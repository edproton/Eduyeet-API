using Infra.Extensions.DependencyInjection;
using Infra.Repositories.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infra;

public static class DependencyInjection
{
    public static IServiceCollection AddInfra(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddConfigurationOptions()
            .AddDynamicDbContext<ApplicationDbContext>(configuration)
            .AddRepositories()
            .AddIdentity();
    }
}