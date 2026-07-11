using GarageFlow.Infrastructure.Auth;
using GarageFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GarageFlow.Infrastructure;

public static class InfrastructureStartupExtensions
{
    public static async Task RunInfrastructureStartupTasksAsync(
        this IServiceProvider serviceProvider,
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        var autoMigrateOnStartup = configuration.GetValue("Database:AutoMigrateOnStartup", true);

        if (autoMigrateOnStartup)
        {
            using var migrateScope = serviceProvider.CreateScope();
            var dbContext = migrateScope.ServiceProvider.GetRequiredService<GarageFlowDbContext>();
            await dbContext.Database.MigrateAsync(cancellationToken);
        }

        using var seedScope = serviceProvider.CreateScope();
        var authSeedService = seedScope.ServiceProvider.GetRequiredService<IAuthUserSeedService>();
        await authSeedService.EnsureSeedAsync(cancellationToken);
    }
}
