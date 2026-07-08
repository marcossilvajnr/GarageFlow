using GarageFlow.Application.Development.Commands;
using GarageFlow.Application.Development.DTOs;
using GarageFlow.Application.Development.Interfaces;

namespace GarageFlow.Application.Development.Handlers;

public sealed class ResetDevelopmentDatabaseHandler(IDevelopmentDatabaseService developmentDatabaseService)
{
    private const string DestructiveOperationBlockedDetail =
        "Operacao destrutiva bloqueada. Envie { \"confirm\": true } para prosseguir.";

    public async Task<DevelopmentDatabaseOperationResult> HandleAsync(
        ResetDevelopmentDatabaseCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!command.Confirm)
            return DevelopmentDatabaseOperationResult.ValidationFailure(DestructiveOperationBlockedDetail);

        return await developmentDatabaseService.ResetAsync(cancellationToken);
    }
}
