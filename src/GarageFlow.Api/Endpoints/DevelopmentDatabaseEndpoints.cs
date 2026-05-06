using GarageFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GarageFlow.Api.Endpoints;

public static class DevelopmentDatabaseEndpoints
{
    public static IEndpointRouteBuilder MapDevelopmentDatabaseEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/dev/database")
            .WithTags("DevDatabase")
            .WithDescription("Operacoes de banco para desenvolvimento local.");

        group.MapPost("/migrate", MigrateDatabase)
            .WithName("MigrateDevelopmentDatabase")
            .WithSummary("Aplica migrations pendentes no banco.")
            .Produces(StatusCodes.Status200OK);

        group.MapPost("/clean", CleanDatabase)
            .WithName("CleanDevelopmentDatabase")
            .WithSummary("Remove todo o banco (destrutivo).")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/reset", ResetDatabase)
            .WithName("ResetDevelopmentDatabase")
            .WithSummary("Limpa e recria o banco com migrations.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        return endpoints;
    }

    private static async Task<IResult> MigrateDatabase(
        GarageFlowDbContext dbContext,
        CancellationToken cancellationToken)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);
        return Results.Ok(new
        {
            message = "Migrations aplicadas com sucesso.",
            provider = dbContext.Database.ProviderName
        });
    }

    private static async Task<IResult> CleanDatabase(
        ConfirmDestructiveOperationRequest request,
        GarageFlowDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (!request.Confirm)
            return Results.BadRequest(new
            {
                message = "Operacao destrutiva bloqueada. Envie { \"confirm\": true } para prosseguir."
            });

        await dbContext.Database.EnsureDeletedAsync(cancellationToken);
        return Results.Ok(new
        {
            message = "Banco removido com sucesso."
        });
    }

    private static async Task<IResult> ResetDatabase(
        ConfirmDestructiveOperationRequest request,
        GarageFlowDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (!request.Confirm)
            return Results.BadRequest(new
            {
                message = "Operacao destrutiva bloqueada. Envie { \"confirm\": true } para prosseguir."
            });

        await dbContext.Database.EnsureDeletedAsync(cancellationToken);
        await dbContext.Database.MigrateAsync(cancellationToken);

        return Results.Ok(new
        {
            message = "Banco recriado com sucesso.",
            provider = dbContext.Database.ProviderName
        });
    }

    private sealed record ConfirmDestructiveOperationRequest(bool Confirm);
}
