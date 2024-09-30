using Application.Repositories;
using Application.Repositories.Shared;
using Application.Services;
using Infra;
using Infra.Repositories.Shared;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application.IntegrationTests.Shared;

[TestClass]
public abstract class IntegrationTestBase
{
    protected static IServiceProvider ServiceProvider;
    protected static IConfiguration Configuration;
    private IServiceScope _scope;

    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext context)
    {
        var services = new ServiceCollection();

        // Load configuration
        Configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.Test.json", optional: false, reloadOnChange: true)
            .Build();

        services.AddSingleton(Configuration);

        // Add application and infrastructure services
        services.AddApplication()
            .AddInfra(Configuration);

        ServiceProvider = services.BuildServiceProvider();
    }

    [TestInitialize]
    public virtual async Task TestInitialize()
    {
        _scope = ServiceProvider.CreateScope();
        ResetDatabase();
        await SeedTestData();
    }

    [TestCleanup]
    public virtual void TestCleanup()
    {
        _scope.Dispose();
    }

    protected ISender Mediator => _scope.ServiceProvider.GetRequiredService<ISender>();

    protected ApplicationDbContext DbContext => _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    protected ITutorRepository TutorRepository => _scope.ServiceProvider.GetRequiredService<ITutorRepository>();
    
    protected IAvailabilityRepository AvailabilityRepository => _scope.ServiceProvider.GetRequiredService<IAvailabilityRepository>();
    
    protected IBookingRepository BookingRepository => _scope.ServiceProvider.GetRequiredService<IBookingRepository>();
    
    protected ILearningSystemRepository LearningSystemRepository => _scope.ServiceProvider.GetRequiredService<ILearningSystemRepository>();
    protected ISubjectRepository SubjectRepository => _scope.ServiceProvider.GetRequiredService<ISubjectRepository>();
    protected IQualificationRepository QualificationRepository => _scope.ServiceProvider.GetRequiredService<IQualificationRepository>();
    
    protected IStudentRepository StudentRepository => _scope.ServiceProvider.GetRequiredService<IStudentRepository>();
    
    protected IPersonRepository PersonRepository => _scope.ServiceProvider.GetRequiredService<IPersonRepository>();
    
    protected IIdentityService IdentityService => _scope.ServiceProvider.GetRequiredService<IIdentityService>();
    
    protected TimeZoneService TimeZoneService => _scope.ServiceProvider.GetRequiredService<TimeZoneService>();
    
    protected IUnitOfWork UnitOfWork => _scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

    protected T GetService<T>() where T : notnull
    {
        return _scope.ServiceProvider.GetRequiredService<T>();
    }

    protected void ResetDatabase()
    {
        DbContext.Database.EnsureDeleted();
        DbContext.Database.EnsureCreated();
    }

    protected virtual Task SeedTestData()
    {
        return Task.CompletedTask;
        // This method can be overridden in derived classes to seed specific test data
    }

    [AssemblyCleanup]
    public static void AssemblyCleanup()
    {
        // Dispose of resources, if any
    }
}