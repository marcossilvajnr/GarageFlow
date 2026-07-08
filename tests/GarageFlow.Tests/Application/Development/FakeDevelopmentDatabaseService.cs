using GarageFlow.Application.Development.DTOs;
using GarageFlow.Application.Development.Interfaces;

namespace GarageFlow.Tests.Application.Development;

internal sealed class FakeDevelopmentDatabaseService : IDevelopmentDatabaseService
{
    public int MigrateCalls { get; private set; }
    public int CleanCalls { get; private set; }
    public int ResetCalls { get; private set; }

    public Task<DevelopmentDatabaseOperationResult> MigrateAsync(CancellationToken cancellationToken = default)
    {
        MigrateCalls++;
        return Task.FromResult(DevelopmentDatabaseOperationResult.Success(
            "Migrations aplicadas com sucesso.",
            "Fake.Provider"));
    }

    public Task<DevelopmentDatabaseOperationResult> CleanAsync(CancellationToken cancellationToken = default)
    {
        CleanCalls++;
        return Task.FromResult(DevelopmentDatabaseOperationResult.Success("Banco removido com sucesso."));
    }

    public Task<DevelopmentDatabaseOperationResult> ResetAsync(CancellationToken cancellationToken = default)
    {
        ResetCalls++;
        return Task.FromResult(DevelopmentDatabaseOperationResult.Success(
            "Banco recriado com sucesso.",
            "Fake.Provider"));
    }
}
