using GarageFlow.Api.Common.Pagination;
using GarageFlow.Api.Supplies.DTOs;
using GarageFlow.Application.Supplies.Commands;
using GarageFlow.Application.Supplies.Handlers;
using GarageFlow.Application.Supplies.Queries;
using Microsoft.AspNetCore.Mvc;

namespace GarageFlow.Api.Supplies.Endpoints;

public static class SuppliesEndpoints
{
    public static IEndpointRouteBuilder MapSupplyEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/supplies")
            .WithTags("Supplies")
            .RequireAuthorization("Administrative");

        group.MapPost("/", CreateSupply)
            .WithName("CreateSupply")
            .WithSummary("Cria um novo insumo.")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<SupplyResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapGet("/{id:guid}", GetSupplyById)
            .WithName("GetSupplyById")
            .WithSummary("Consulta insumo por Id.")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<SupplyResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapGet("/", ListSupplies)
            .WithName("ListSupplies")
            .WithSummary("Lista insumos com paginação.")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<PagedSupplyResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", UpdateSupply)
            .WithName("UpdateSupply")
            .WithSummary("Atualiza dados do insumo.")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<SupplyResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}", DeactivateSupply)
            .WithName("DeactivateSupply")
            .WithSummary("Desativa insumo (soft delete).")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        return endpoints;
    }

    private static async Task<IResult> CreateSupply(
        CreateSupplyRequest request,
        CreateSupplyHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new CreateSupplyCommand(
            request.Name,
            request.Code,
            request.UnitOfMeasure,
            request.BaseCost,
            request.PreferredSupplierId);
        var dto = await handler.HandleAsync(command, cancellationToken);
        return Results.Created($"/supplies/{dto.Id}", MapToResponse(dto));
    }

    private static async Task<IResult> GetSupplyById(
        Guid id,
        GetSupplyByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var dto = await handler.HandleAsync(new GetSupplyByIdQuery(id), cancellationToken);
        return Results.Ok(MapToResponse(dto));
    }

    private static async Task<IResult> ListSupplies(
        ListSuppliesHandler handler,
        CancellationToken cancellationToken,
        int page = ApiPagination.DefaultPage,
        int pageSize = ApiPagination.DefaultPageSize)
    {
        if (!ApiPagination.IsValid(page, pageSize))
        {
            return Results.BadRequest(ApiPagination.CreateInvalidPaginationProblemDetails());
        }

        var result = await handler.HandleAsync(new ListSuppliesQuery(page, pageSize), cancellationToken);
        var response = new PagedSupplyResponse(
            result.Items.Select(MapToResponse).ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount);

        return Results.Ok(response);
    }

    private static async Task<IResult> UpdateSupply(
        Guid id,
        UpdateSupplyRequest request,
        UpdateSupplyHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSupplyCommand(id, request.Name, request.UnitOfMeasure, request.BaseCost, request.PreferredSupplierId);
        var dto = await handler.HandleAsync(command, cancellationToken);
        return Results.Ok(MapToResponse(dto));
    }

    private static async Task<IResult> DeactivateSupply(
        Guid id,
        DeactivateSupplyHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new DeactivateSupplyCommand(id), cancellationToken);
        return Results.NoContent();
    }

    private static SupplyResponse MapToResponse(Application.Supplies.DTOs.SupplyDto dto) => new(
        dto.Id,
        dto.Name,
        dto.Code,
        dto.UnitOfMeasure,
        dto.BaseCost,
        dto.PreferredSupplierId,
        dto.IsActive,
        dto.CreatedAt,
        dto.UpdatedAt);
}
