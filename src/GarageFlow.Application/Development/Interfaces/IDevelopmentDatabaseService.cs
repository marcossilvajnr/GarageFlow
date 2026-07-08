using GarageFlow.Application.Development.DTOs;

namespace GarageFlow.Application.Development.Interfaces;

public interface IDevelopmentDatabaseService
{
    Task<DevelopmentDatabaseOperationResult> MigrateAsync(CancellationToken cancellationToken = default);
    Task<DevelopmentDatabaseOperationResult> CleanAsync(CancellationToken cancellationToken = default);
    Task<DevelopmentDatabaseOperationResult> ResetAsync(CancellationToken cancellationToken = default);
}
