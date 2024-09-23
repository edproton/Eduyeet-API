using System.ComponentModel.DataAnnotations;

namespace Infra.Options;

public enum EnvironmentType
{
    Development,
    Staging,
    Production
}

public class EnvironmentOptions
{
    [Required]
    public EnvironmentType Environment { get; set; }
    
    public bool IsDevelopment => Environment == EnvironmentType.Development;
    
    public bool IsStaging => Environment == EnvironmentType.Staging;
    
    public bool IsProduction => Environment == EnvironmentType.Production;
}