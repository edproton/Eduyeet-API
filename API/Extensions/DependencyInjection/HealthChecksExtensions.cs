using System.Diagnostics;
using API.Utils;
using Infra.Options;
using Infra.Repositories.Shared;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace API.Extensions.DependencyInjection;

public static class HealthChecksExtensions
{
    public static IServiceCollection AddHealthChecksServices(
        this IServiceCollection services)
    {
        var maxWorkingSet = 1.GB();

        services
            .AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy())
            .AddDbContextCheck<ApplicationDbContext>()
            .AddNpgSql(sp =>
            {
                var databaseOptions = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
                return databaseOptions.GetConnectionString();
            })
            .AddCheck("memory", CheckMemory)
            .AddCheck("cpu", CheckCPU)
            .AddProcessAllocatedMemoryHealthCheck((int)maxWorkingSet.Bytes)
            .AddProcessHealthCheck("dotnet", p => p.Length > 0);

        return services;
    }

    private static HealthCheckResult CheckMemory()
    {
        var totalMemory = MemorySize.FromBytes(GC.GetTotalMemory(false));
        var memoryInfo = GC.GetGCMemoryInfo();
        var totalAvailableMemory = MemorySize.FromBytes(memoryInfo.TotalAvailableMemoryBytes);
        
        var memoryUsagePercentage = (double)totalMemory.Bytes / totalAvailableMemory.Bytes * 100;
        
        var status = memoryUsagePercentage < 80 ? HealthStatus.Healthy : HealthStatus.Degraded;
        
        return new HealthCheckResult(
            status,
            description: $"Memory usage: {memoryUsagePercentage:F2}% ({totalMemory} / {totalAvailableMemory})"
        );
    }

    private static HealthCheckResult CheckCPU()
    {
        var cpuUsage = GetCpuUsage();
        var status = cpuUsage < 80 ? HealthStatus.Healthy : HealthStatus.Degraded;
        
        return new HealthCheckResult(
            status,
            description: $"CPU usage: {cpuUsage:F2}%"
        );
    }

    private static float GetCpuUsage()
    {
        var startTime = DateTime.UtcNow;
        var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;

        Thread.Sleep(500);

        var endTime = DateTime.UtcNow;
        var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;

        var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
        var totalMsPassed = (endTime - startTime).TotalMilliseconds;
        var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

        return (float)(cpuUsageTotal * 100);
    }
}