using GarageFlow.Application.Development.DTOs;
using GarageFlow.Application.Development.Interfaces;
using GarageFlow.Infrastructure.Auth;
using GarageFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GarageFlow.Infrastructure.Development;

internal sealed class DevelopmentDatabaseService(
    GarageFlowDbContext dbContext,
    IAuthUserSeedService authUserSeedService) : IDevelopmentDatabaseService
{
    public async Task<DevelopmentDatabaseOperationResult> MigrateAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);
        await authUserSeedService.EnsureSeedAsync(cancellationToken);

        return DevelopmentDatabaseOperationResult.Success(
            "Migrations aplicadas com sucesso.",
            dbContext.Database.ProviderName);
    }

    public async Task<DevelopmentDatabaseOperationResult> CleanAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureDeletedAsync(cancellationToken);

        return DevelopmentDatabaseOperationResult.Success("Banco removido com sucesso.");
    }

    public async Task<DevelopmentDatabaseOperationResult> ResetAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureDeletedAsync(cancellationToken);
        await dbContext.Database.MigrateAsync(cancellationToken);
        await authUserSeedService.EnsureSeedAsync(cancellationToken);

        return DevelopmentDatabaseOperationResult.Success(
            "Banco recriado com sucesso.",
            dbContext.Database.ProviderName);
    }
}
