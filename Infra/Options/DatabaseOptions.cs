using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Npgsql;

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

    public string GetConnectionString()
    {
        return Provider switch
        {
            DatabaseProvider.Postgres => BuildPostgresConnectionString(Postgres),
            DatabaseProvider.SqlServer => BuildSqlServerConnectionString(SqlServer),
            DatabaseProvider.Sqlite => BuildSqliteConnectionString(Sqlite),
            _ => throw new ArgumentException($"Unsupported database provider: {Provider}")
        };
    }
    
    private static string BuildPostgresConnectionString(PostgresOptions options)
    {
        return new NpgsqlConnectionStringBuilder
        {
            Host = options.Host,
            Port = options.Port,
            Database = options.Database,
            Username = options.Username,
            Password = options.Password
        }.ToString();
    }

    private static string BuildSqlServerConnectionString(SqlServerOptions options)
    {
        return new SqlConnectionStringBuilder
        {
            DataSource = $"{options.Host},{options.Port}",
            InitialCatalog = options.Database,
            UserID = options.Username,
            Password = options.Password
        }.ToString();
    }

    private static string BuildSqliteConnectionString(SqliteOptions options)
    {
        return new SqliteConnectionStringBuilder
        {
            DataSource = options.Filename
        }.ToString();
    }
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