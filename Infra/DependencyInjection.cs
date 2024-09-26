using Application.Services;
using Infra.Extensions.DependencyInjection;
using Infra.Options;
using Infra.Repositories.Shared;
using Infra.Services;
using Infra.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infra;

public static class DependencyInjection
{
    public static IServiceCollection AddInfra(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddConfigurationOptions()
            .AddDynamicDbContext<ApplicationDbContext>(configuration)
            .AddRepositories()
            .AddServices()
            .AddIdentity();
    }
}