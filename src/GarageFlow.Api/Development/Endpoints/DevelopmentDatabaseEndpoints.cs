using GarageFlow.Api.Common.Responses;
using GarageFlow.Infrastructure.Persistence;
using GarageFlow.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace GarageFlow.Api.Development.Endpoints;

public static class DevelopmentDatabaseEndpoints
{
    private const string DestructiveOperationBlockedDetail =
        "Operacao destrutiva bloqueada. Envie { \"confirm\": true } para prosseguir.";

    public static IEndpointRouteBuilder MapDevelopmentDatabaseEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/dev/database")
            .WithTags("DevDatabase")
            .WithDescription("Operacoes de banco para desenvolvimento local.")
            .RequireAuthorization("Administrative");

        group.MapPost("/migrate", MigrateDatabase)
            .WithName("MigrateDevelopmentDatabase")
            .WithSummary("Aplica migrations pendentes no banco.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPost("/clean", CleanDatabase)
            .WithName("CleanDevelopmentDatabase")
            .WithSummary("Remove todo o banco (destrutivo).")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPost("/reset", ResetDatabase)
            .WithName("ResetDevelopmentDatabase")
            .WithSummary("Limpa e recria o banco com migrations.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        return endpoints;
    }

    private static async Task<IResult> MigrateDatabase(
        GarageFlowDbContext dbContext,
        IAuthUserSeedService authUserSeedService,
        CancellationToken cancellationToken)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);
        await authUserSeedService.EnsureSeedAsync(cancellationToken);
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
            return Results.BadRequest(ApiProblemDetails.CreateValidationProblemDetails(DestructiveOperationBlockedDetail));

        await dbContext.Database.EnsureDeletedAsync(cancellationToken);
        return Results.Ok(new
        {
            message = "Banco removido com sucesso."
        });
    }

    private static async Task<IResult> ResetDatabase(
        ConfirmDestructiveOperationRequest request,
        GarageFlowDbContext dbContext,
        IAuthUserSeedService authUserSeedService,
        CancellationToken cancellationToken)
    {
        if (!request.Confirm)
            return Results.BadRequest(ApiProblemDetails.CreateValidationProblemDetails(DestructiveOperationBlockedDetail));

        await dbContext.Database.EnsureDeletedAsync(cancellationToken);
        await dbContext.Database.MigrateAsync(cancellationToken);
        await authUserSeedService.EnsureSeedAsync(cancellationToken);

        return Results.Ok(new
        {
            message = "Banco recriado com sucesso.",
            provider = dbContext.Database.ProviderName
        });
    }

    private sealed record ConfirmDestructiveOperationRequest(bool Confirm);
}
