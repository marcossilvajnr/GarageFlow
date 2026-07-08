using GarageFlow.Application.Development.Commands;
using GarageFlow.Application.Development.DTOs;
using GarageFlow.Application.Development.Interfaces;

namespace GarageFlow.Application.Development.Handlers;

public sealed class MigrateDevelopmentDatabaseHandler(IDevelopmentDatabaseService developmentDatabaseService)
{
    public async Task<DevelopmentDatabaseOperationResult> HandleAsync(
        MigrateDevelopmentDatabaseCommand command,
        CancellationToken cancellationToken = default)
        => await developmentDatabaseService.MigrateAsync(cancellationToken);
}
