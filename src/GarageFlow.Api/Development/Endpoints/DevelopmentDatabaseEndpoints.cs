using GarageFlow.Api.Common.Authorization;
using GarageFlow.Api.Common.Responses;
using GarageFlow.Application.Development.Commands;
using GarageFlow.Application.Development.DTOs;
using GarageFlow.Application.Development.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace GarageFlow.Api.Development.Endpoints;

public static class DevelopmentDatabaseEndpoints
{
    public static IEndpointRouteBuilder MapDevelopmentDatabaseEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/dev/database")
            .WithTags("DevDatabase")
            .WithDescription("Operacoes de banco para desenvolvimento local.")
            .RequireRoles(ApiRoles.Administrative);

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
        MigrateDevelopmentDatabaseHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new MigrateDevelopmentDatabaseCommand(), cancellationToken);

        return Results.Ok(new
        {
            message = result.Message,
            provider = result.Provider
        });
    }

    private static async Task<IResult> CleanDatabase(
        ConfirmDestructiveOperationRequest request,
        CleanDevelopmentDatabaseHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new CleanDevelopmentDatabaseCommand(request.Confirm), cancellationToken);
        if (!result.IsSuccess)
            return CreateValidationProblem(result);

        return Results.Ok(new
        {
            message = result.Message
        });
    }

    private static async Task<IResult> ResetDatabase(
        ConfirmDestructiveOperationRequest request,
        ResetDevelopmentDatabaseHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new ResetDevelopmentDatabaseCommand(request.Confirm), cancellationToken);
        if (!result.IsSuccess)
            return CreateValidationProblem(result);

        return Results.Ok(new
        {
            message = result.Message,
            provider = result.Provider
        });
    }

    private static IResult CreateValidationProblem(DevelopmentDatabaseOperationResult result)
        => Results.BadRequest(ApiProblemDetails.CreateValidationProblemDetails(result.ValidationDetail!));

    private sealed record ConfirmDestructiveOperationRequest(bool Confirm);
}
