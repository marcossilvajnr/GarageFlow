using GarageFlow.Api.Common.Pagination;
using GarageFlow.Api.Stock.DTOs;
using GarageFlow.Application.Stock.Commands;
using GarageFlow.Application.Stock.DTOs;
using GarageFlow.Application.Stock.Handlers;
using GarageFlow.Application.Stock.Queries;
using Microsoft.AspNetCore.Mvc;

namespace GarageFlow.Api.Stock.Endpoints;

public static class SeparationOrdersEndpoints
{
    public static IEndpointRouteBuilder MapSeparationOrderEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/separation-orders").WithTags("SeparationOrders");

        group.MapPost("/", CreateSeparationOrder)
            .WithName("CreateSeparationOrder")
            .WithSummary("Cria uma nova Ordem de Separação.")
            .RequireAuthorization("StockistOrAdministrative")
            .Produces<SeparationOrderResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:guid}", GetSeparationOrderById)
            .WithName("GetSeparationOrderById")
            .WithSummary("Consulta Ordem de Separação por Id.")
            .RequireAuthorization("StockistOrMechanicOrAdministrative")
            .Produces<SeparationOrderResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapGet("/", ListSeparationOrders)
            .WithName("ListSeparationOrders")
            .WithSummary("Lista Ordens de Separação com paginação.")
            .RequireAuthorization("StockistOrMechanicOrAdministrative")
            .Produces<PagedSeparationOrderResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:guid}/reserve", ReserveSeparationOrder)
            .WithName("ReserveSeparationOrder")
            .WithSummary("Reserva os itens da Ordem de Separação.")
            .RequireAuthorization("StockistOrAdministrative")
            .Produces<SeparationOrderResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/return-total", ReturnSeparationOrderTotal)
            .WithName("ReturnSeparationOrderTotal")
            .WithSummary("Realiza devolução total dos itens da Ordem de Separação antes do recebimento do mecânico.")
            .RequireAuthorization("StockistOrAdministrative")
            .Produces<SeparationOrderResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/wait-purchase", WaitSeparationOrderPurchase)
            .WithName("WaitSeparationOrderPurchase")
            .WithSummary("Coloca a Ordem de Separação em aguardo de compra.")
            .RequireAuthorization("StockistOrAdministrative")
            .Produces<SeparationOrderResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/resume-after-purchase", ResumeSeparationOrderAfterPurchase)
            .WithName("ResumeSeparationOrderAfterPurchase")
            .WithSummary("Retoma a Ordem de Separação após compra concluída.")
            .RequireAuthorization("StockistOrAdministrative")
            .Produces<SeparationOrderResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/confirm-stockist-withdrawal", ConfirmStockistWithdrawal)
            .WithName("ConfirmSeparationStockistWithdrawal")
            .WithSummary("Confirma retirada física pelo estoquista.")
            .RequireAuthorization("StockistOrAdministrative")
            .Produces<SeparationOrderResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/confirm-mechanic-receipt", ConfirmMechanicReceipt)
            .WithName("ConfirmSeparationMechanicReceipt")
            .WithSummary("Confirma recebimento pelo mecânico.")
            .RequireAuthorization("MechanicOrAdministrative")
            .Produces<SeparationOrderResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        return endpoints;
    }

    private static async Task<IResult> CreateSeparationOrder(
        CreateSeparationOrderRequest request,
        CreateSeparationOrderHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new CreateSeparationOrderCommand(
            request.ExecutionOrderId,
            (request.Parts ?? [])
                .Select(p => new CreateSeparationPartItemCommand(p.PartId, p.PartName, p.Quantity))
                .ToList(),
            (request.Supplies ?? [])
                .Select(s => new CreateSeparationSupplyItemCommand(s.SupplyId, s.SupplyName, s.Quantity, s.Unit))
                .ToList());

        var dto = await handler.HandleAsync(command, cancellationToken);
        var response = MapToResponse(dto);
        return Results.Created($"/separation-orders/{dto.Id}", response);
    }

    private static async Task<IResult> GetSeparationOrderById(
        Guid id,
        GetSeparationOrderByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var dto = await handler.HandleAsync(new GetSeparationOrderByIdQuery(id), cancellationToken);
        return Results.Ok(MapToResponse(dto));
    }

    private static async Task<IResult> ListSeparationOrders(
        ListSeparationOrdersHandler handler,
        CancellationToken cancellationToken,
        int page = ApiPagination.DefaultPage,
        int pageSize = ApiPagination.DefaultPageSize)
    {
        if (!ApiPagination.IsValid(page, pageSize))
        {
            return Results.BadRequest(ApiPagination.CreateInvalidPaginationProblemDetails());
        }

        var result = await handler.HandleAsync(new ListSeparationOrdersQuery(page, pageSize), cancellationToken);
        var response = new PagedSeparationOrderResponse(
            result.Items.Select(MapToResponse).ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount);

        return Results.Ok(response);
    }

    private static async Task<IResult> ReserveSeparationOrder(
        Guid id,
        ReserveSeparationOrderHandler handler,
        CancellationToken cancellationToken)
    {
        var dto = await handler.HandleAsync(new ReserveSeparationOrderCommand(id), cancellationToken);
        return Results.Ok(MapToResponse(dto));
    }

    private static async Task<IResult> WaitSeparationOrderPurchase(
        Guid id,
        WaitSeparationOrderPurchaseHandler handler,
        CancellationToken cancellationToken)
    {
        var dto = await handler.HandleAsync(new WaitSeparationOrderPurchaseCommand(id), cancellationToken);
        return Results.Ok(MapToResponse(dto));
    }

    private static async Task<IResult> ReturnSeparationOrderTotal(
        Guid id,
        ReturnSeparationOrderTotalHandler handler,
        CancellationToken cancellationToken)
    {
        var dto = await handler.HandleAsync(new ReturnSeparationOrderTotalCommand(id), cancellationToken);
        return Results.Ok(MapToResponse(dto));
    }

    private static async Task<IResult> ResumeSeparationOrderAfterPurchase(
        Guid id,
        ResumeSeparationOrderAfterPurchaseHandler handler,
        CancellationToken cancellationToken)
    {
        var dto = await handler.HandleAsync(new ResumeSeparationOrderAfterPurchaseCommand(id), cancellationToken);
        return Results.Ok(MapToResponse(dto));
    }

    private static async Task<IResult> ConfirmStockistWithdrawal(
        Guid id,
        ConfirmSeparationStockistWithdrawalRequest request,
        ConfirmSeparationStockistWithdrawalHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new ConfirmSeparationStockistWithdrawalCommand(id, request.StockistId);
        var dto = await handler.HandleAsync(command, cancellationToken);
        return Results.Ok(MapToResponse(dto));
    }

    private static async Task<IResult> ConfirmMechanicReceipt(
        Guid id,
        ConfirmSeparationMechanicReceiptHandler handler,
        CancellationToken cancellationToken)
    {
        var dto = await handler.HandleAsync(new ConfirmSeparationMechanicReceiptCommand(id), cancellationToken);
        return Results.Ok(MapToResponse(dto));
    }

    private static SeparationOrderResponse MapToResponse(SeparationOrderDto dto) =>
        new(
            dto.Id,
            dto.ExecutionOrderId,
            dto.Status,
            dto.Parts.Select(p => new SeparationPartItemResponse(p.PartId, p.PartName, p.Quantity, p.IsReserved)).ToList(),
            dto.Supplies.Select(s => new SeparationSupplyItemResponse(s.SupplyId, s.SupplyName, s.Quantity, s.Unit, s.IsReserved)).ToList(),
            dto.StockistId,
            dto.ConfirmedByStockistAt,
            dto.ConfirmedByMechanicAt,
            dto.CreatedAt);
}
