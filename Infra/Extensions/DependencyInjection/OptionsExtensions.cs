using Microsoft.Extensions.DependencyInjection;

namespace Infra.Extensions.DependencyInjection;

public static class OptionsExtensions
{
    public static IServiceCollection AddOptionsWithValidation<TOptions>(
        this IServiceCollection services,
        string? configurationSection = null) 
        where TOptions : class
    {
        services.AddOptions<TOptions>()
            .BindConfiguration(configurationSection ?? typeof(TOptions).Name.Replace("Options", string.Empty))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        return services;
    }
}