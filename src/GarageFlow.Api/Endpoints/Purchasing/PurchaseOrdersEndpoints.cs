using GarageFlow.Api.DTOs.Purchasing;
using GarageFlow.Application.Purchasing.Commands;
using GarageFlow.Application.Purchasing.DTOs;
using GarageFlow.Application.Purchasing.Handlers;
using GarageFlow.Application.Purchasing.Queries;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;
using Microsoft.AspNetCore.Mvc;

namespace GarageFlow.Api.Endpoints.Purchasing;

public static class PurchaseOrdersEndpoints
{
    public static IEndpointRouteBuilder MapPurchaseOrderEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/purchase-orders").WithTags("PurchaseOrders");

        group.MapPost("/", CreatePurchaseOrder)
            .WithName("CreatePurchaseOrder")
            .WithSummary("Cria uma nova Ordem de Compra.")
            .RequireAuthorization("StockistOrAdministrative")
            .Produces<PurchaseOrderResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:guid}", GetPurchaseOrderById)
            .WithName("GetPurchaseOrderById")
            .WithSummary("Consulta Ordem de Compra por Id.")
            .Produces<PurchaseOrderResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapGet("/", ListPurchaseOrders)
            .WithName("ListPurchaseOrders")
            .WithSummary("Lista Ordens de Compra com paginação.")
            .Produces<PagedPurchaseOrderResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:guid}/assign-supplier", AssignSupplier)
            .WithName("AssignPurchaseOrderSupplier")
            .WithSummary("Atribui fornecedor à Ordem de Compra.")
            .RequireAuthorization("StockistOrAdministrative")
            .Produces<PurchaseOrderResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/start", StartPurchaseOrder)
            .WithName("StartPurchaseOrder")
            .WithSummary("Inicia a Ordem de Compra.")
            .RequireAuthorization("StockistOrAdministrative")
            .Produces<PurchaseOrderResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/complete", CompletePurchaseOrder)
            .WithName("CompletePurchaseOrder")
            .WithSummary("Conclui a Ordem de Compra.")
            .RequireAuthorization("StockistOrAdministrative")
            .Produces<PurchaseOrderResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        return endpoints;
    }

    private static async Task<IResult> CreatePurchaseOrder(
        CreatePurchaseOrderRequest request,
        CreatePurchaseOrderHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreatePurchaseOrderCommand(
                (request.SeparationOrderIds ?? []).ToList(),
                (request.Items ?? [])
                    .Select(i => new CreatePurchaseItemCommand(i.ItemId, i.ItemType, i.ItemName, i.Quantity, i.UnitPrice))
                    .ToList());

            var dto = await handler.HandleAsync(command, cancellationToken);
            var response = MapToResponse(dto);
            return Results.Created($"/purchase-orders/{dto.Id}", response);
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(new ProblemDetails { Title = "Erro de validação", Detail = ex.Message, Status = 400 });
        }
    }

    private static async Task<IResult> GetPurchaseOrderById(
        Guid id,
        GetPurchaseOrderByIdHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var dto = await handler.HandleAsync(new GetPurchaseOrderByIdQuery(id), cancellationToken);
            return Results.Ok(MapToResponse(dto));
        }
        catch (EntityNotFoundException ex)
        {
            return Results.NotFound(new ProblemDetails { Title = "Não encontrado", Detail = ex.Message, Status = 404 });
        }
    }

    private static async Task<IResult> ListPurchaseOrders(
        ListPurchaseOrdersHandler handler,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 20)
    {
        if (page < 1 || pageSize < 1)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Parâmetros de paginação inválidos",
                Detail = DomainErrorMessages.InvalidPaginationParameters,
                Status = 400
            });
        }

        var result = await handler.HandleAsync(new ListPurchaseOrdersQuery(page, pageSize), cancellationToken);
        var response = new PagedPurchaseOrderResponse(
            result.Items.Select(MapToResponse).ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount);

        return Results.Ok(response);
    }

    private static async Task<IResult> AssignSupplier(
        Guid id,
        AssignPurchaseOrderSupplierRequest request,
        AssignPurchaseOrderSupplierHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new AssignPurchaseOrderSupplierCommand(id, request.SupplierId);
            var dto = await handler.HandleAsync(command, cancellationToken);
            return Results.Ok(MapToResponse(dto));
        }
        catch (InvalidPurchaseOrderStatusTransitionException ex)
        {
            return Results.Conflict(new ProblemDetails { Title = "Conflito de estado", Detail = ex.Message, Status = 409 });
        }
        catch (EntityNotFoundException ex)
        {
            return Results.NotFound(new ProblemDetails { Title = "Não encontrado", Detail = ex.Message, Status = 404 });
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(new ProblemDetails { Title = "Erro de validação", Detail = ex.Message, Status = 400 });
        }
    }

    private static async Task<IResult> StartPurchaseOrder(
        Guid id,
        StartPurchaseOrderHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var dto = await handler.HandleAsync(new StartPurchaseOrderCommand(id), cancellationToken);
            return Results.Ok(MapToResponse(dto));
        }
        catch (InvalidPurchaseOrderStatusTransitionException ex)
        {
            return Results.Conflict(new ProblemDetails { Title = "Conflito de estado", Detail = ex.Message, Status = 409 });
        }
        catch (EntityNotFoundException ex)
        {
            return Results.NotFound(new ProblemDetails { Title = "Não encontrado", Detail = ex.Message, Status = 404 });
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(new ProblemDetails { Title = "Erro de validação", Detail = ex.Message, Status = 400 });
        }
    }

    private static async Task<IResult> CompletePurchaseOrder(
        Guid id,
        CompletePurchaseOrderHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var dto = await handler.HandleAsync(new CompletePurchaseOrderCommand(id), cancellationToken);
            return Results.Ok(MapToResponse(dto));
        }
        catch (InvalidSeparationOrderStatusTransitionException ex)
        {
            return Results.Conflict(new ProblemDetails { Title = "Conflito de estado", Detail = ex.Message, Status = 409 });
        }
        catch (InvalidPurchaseOrderStatusTransitionException ex)
        {
            return Results.Conflict(new ProblemDetails { Title = "Conflito de estado", Detail = ex.Message, Status = 409 });
        }
        catch (EntityNotFoundException ex)
        {
            return Results.NotFound(new ProblemDetails { Title = "Não encontrado", Detail = ex.Message, Status = 404 });
        }
    }

    private static PurchaseOrderResponse MapToResponse(PurchaseOrderDto dto) =>
        new(
            dto.Id,
            dto.SeparationOrderIds,
            dto.SupplierId,
            dto.Status,
            dto.Items.Select(i => new PurchaseItemResponse(i.ItemId, i.ItemType, i.ItemName, i.Quantity, i.UnitPrice, i.Subtotal)).ToList(),
            dto.CreatedAt,
            dto.StartedAt,
            dto.CompletedAt);
}
