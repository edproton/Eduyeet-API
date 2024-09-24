using Application.Repositories;
using Application.Repositories.Shared;
using Infra.Options;
using Infra.Repositories;
using Infra.Repositories.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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

        services.AddDbContext<TContext>((sp, options) =>
        {
            var dbOptions = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            var connectionString = dbOptions.GetConnectionString();
        
            switch (dbOptions.Provider)
            {
                case DatabaseProvider.Postgres:
                    options.UseNpgsql(connectionString);
                    break;
                case DatabaseProvider.SqlServer:
                    options.UseSqlServer(connectionString);
                    break;
                case DatabaseProvider.Sqlite:
                    options.UseSqlite(connectionString);
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
}