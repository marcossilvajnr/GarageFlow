using GarageFlow.Api.Common.Authorization;
using GarageFlow.Api.ServiceOrders.DTOs;
using GarageFlow.Api.ServiceOrders.Endpoints;
using GarageFlow.Application.ServiceOrders.Commands;
using GarageFlow.Application.ServiceOrders.Handlers;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace GarageFlow.Api.ServiceOrders.External;

public static class ExternalServiceOrderQuoteEndpoints
{
    public static IEndpointRouteBuilder MapExternalServiceOrderQuoteEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/external").WithTags("ExternalIntegrations");

        group.MapPost("/service-order-quote-notifications", HandleExternalQuoteDecision)
            .WithName("HandleExternalQuoteDecision")
            .WithSummary("Recebe a decisão externa (aprovação/recusa) do orçamento da Ordem de Serviço.")
            .RequireRoles(ApiRoles.External)
            .Produces<QuoteResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        return endpoints;
    }

    private static async Task<IResult> HandleExternalQuoteDecision(
        ExternalQuoteDecisionNotificationRequest request,
        HandleExternalQuoteDecisionHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new HandleExternalQuoteDecisionCommand(
            request.ServiceOrderId,
            GetDecisionText(request.Decision),
            request.Reason,
            request.ExternalNotificationId,
            request.Source);

        var dto = await handler.HandleAsync(command, cancellationToken);
        return Results.Ok(ServiceOrdersEndpoints.MapToQuoteResponse(dto));
    }

    private static string? GetDecisionText(JsonElement decision) =>
        decision.ValueKind switch
        {
            JsonValueKind.String => decision.GetString(),
            JsonValueKind.Undefined or JsonValueKind.Null => null,
            _ => decision.GetRawText()
        };
}
