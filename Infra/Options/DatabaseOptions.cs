using System.ComponentModel.DataAnnotations;

namespace Infra.Options;

public enum DatabaseProvider
{
    Postgres,
    SqlServer,
    Sqlite
}

public class DatabaseOptions
{
    [Required]
    public required bool MigrateOnStart { get; init; }

    [Required]
    public required DatabaseProvider Provider { get; init; }

    [Required]
    public required PostgresOptions Postgres { get; init; }

    [Required]
    public required SqlServerOptions SqlServer { get; init; }

    [Required]
    public required SqliteOptions Sqlite { get; init; }
}

public class PostgresOptions
{
    [Required]
    public required string Host { get; init; }

    [Required]
    public required int Port { get; init; }

    [Required]
    public required string Database { get; init; }

    [Required]
    public required string Username { get; init; }

    [Required]
    public required string Password { get; init; }
}

public class SqlServerOptions
{
    [Required]
    public required string Host { get; init; }

    [Required]
    public required int Port { get; init; }

    [Required]
    public required string Database { get; init; }

    [Required]
    public required string Username { get; init; }

    [Required]
    public required string Password { get; init; }
}

public class SqliteOptions
{
    [Required]
    public required string Filename { get; init; }
}