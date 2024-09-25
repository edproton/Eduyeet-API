using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace API.Options;

public class CorsOptions
{
    [Required]
    public required string[] AllowedOrigins { get; init; }

    [Required]
    public required string[] AllowedMethods { get; init; }

    [Required]
    public required string[] AllowedHeaders { get; init; }
}

public class CorsOptionsConfigure(
    IOptions<CorsOptions> corsOptions) : IConfigureOptions<Microsoft.AspNetCore.Cors.Infrastructure.CorsOptions>
{
    public const string CorsPolicy = "CorsPolicy";

    private readonly CorsOptions _corsOptions = corsOptions.Value;

    public void Configure(Microsoft.AspNetCore.Cors.Infrastructure.CorsOptions options)
    {
        options.AddPolicy(CorsPolicy,
            policy =>
            {
                policy.WithOrigins(_corsOptions.AllowedOrigins)
                    .WithMethods(_corsOptions.AllowedMethods)
                    .WithHeaders(_corsOptions.AllowedHeaders);
            });
    }
}
