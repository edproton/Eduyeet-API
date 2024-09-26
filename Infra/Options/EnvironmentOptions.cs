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
    public string Type
    {
        get => EnvironmentType.ToString();
        set
        {
            if (Enum.TryParse<EnvironmentType>(value, true, out var parsedType))
            {
                EnvironmentType = parsedType;
            }
            else
            {
                throw new ArgumentException(
                    $"Invalid environment type: {value}. Must be one of: {string.Join(", ", Enum.GetNames<EnvironmentType>())}");
            }
        }
    }

    public EnvironmentType EnvironmentType { get; private set; }

    public bool IsDevelopment => EnvironmentType == EnvironmentType.Development;
    
    public bool IsStaging => EnvironmentType == EnvironmentType.Staging;
    
    public bool IsProduction => EnvironmentType == EnvironmentType.Production;
}