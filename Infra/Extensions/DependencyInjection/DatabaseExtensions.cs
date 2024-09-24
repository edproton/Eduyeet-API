using Application.Repositories;
using Application.Repositories.Shared;
using Infra.Options;
using Infra.Repositories;
using Infra.Repositories.Shared;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Infra.Extensions.DependencyInjection;

public static class DatabaseExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ILearningSystemRepository, LearningSystemRepository>();
        services.AddScoped<IQualificationRepository, QualificationRepository>();
        services.AddScoped<ISubjectRepository, SubjectRepository>();
        services.AddScoped<IPersonRepository, PersonRepository>()
            .AddScoped<ITutorRepository, TutorRepository>()
            .AddScoped<IStudentRepository, StudentRepository>();

        return services;
    }

    public static IServiceCollection AddDynamicDbContext<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string configurationSection = "Database")
        where TContext : DbContext
    {
        var dbOptions = configuration.GetSection(configurationSection).Get<DatabaseOptions>();

        services.AddDbContext<TContext>((_, options) =>
        {
            switch (dbOptions!.Provider)
            {
                case DatabaseProvider.Postgres:
                    options.UseNpgsql(BuildPostgresConnectionString(dbOptions.Postgres));
                    break;
                case DatabaseProvider.SqlServer:
                    options.UseSqlServer(BuildSqlServerConnectionString(dbOptions.SqlServer));
                    break;
                case DatabaseProvider.Sqlite:
                    options.UseSqlite(BuildSqliteConnectionString(dbOptions.Sqlite));
                    break;
                default:
                    throw new ArgumentException($"Unsupported database provider: {dbOptions.Provider}");
            }
        });

        if (dbOptions!.MigrateOnStart)
        {
            using var scope = services.BuildServiceProvider().CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TContext>();
            context.Database.Migrate();
        }

        return services;
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