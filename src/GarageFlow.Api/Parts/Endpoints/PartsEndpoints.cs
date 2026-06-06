using GarageFlow.Api.Common.Pagination;
using GarageFlow.Api.Parts.DTOs;
using GarageFlow.Application.Parts.Commands;
using GarageFlow.Application.Parts.Handlers;
using GarageFlow.Application.Parts.Queries;
using Microsoft.AspNetCore.Mvc;

namespace GarageFlow.Api.Parts.Endpoints;

public static class PartsEndpoints
{
    public static IEndpointRouteBuilder MapPartEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/parts")
            .WithTags("Parts")
            .RequireAuthorization("Administrative");

        group.MapPost("/", CreatePart)
            .WithName("CreatePart")
            .WithSummary("Cria uma nova peça.")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<PartResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapGet("/{id:guid}", GetPartById)
            .WithName("GetPartById")
            .WithSummary("Consulta peça por Id.")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<PartResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapGet("/", ListParts)
            .WithName("ListParts")
            .WithSummary("Lista peças com paginação.")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<PagedPartResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", UpdatePart)
            .WithName("UpdatePart")
            .WithSummary("Atualiza dados da peça.")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<PartResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}", DeactivatePart)
            .WithName("DeactivatePart")
            .WithSummary("Desativa peça (soft delete).")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        return endpoints;
    }

    private static async Task<IResult> CreatePart(
        CreatePartRequest request,
        CreatePartHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new CreatePartCommand(
            request.Name,
            request.Code,
            request.Sku,
            request.UnitOfMeasure,
            request.UnitPrice);
        var dto = await handler.HandleAsync(command, cancellationToken);
        return Results.Created($"/parts/{dto.Id}", MapToResponse(dto));
    }

    private static async Task<IResult> GetPartById(
        Guid id,
        GetPartByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var dto = await handler.HandleAsync(new GetPartByIdQuery(id), cancellationToken);
        return Results.Ok(MapToResponse(dto));
    }

    private static async Task<IResult> ListParts(
        ListPartsHandler handler,
        CancellationToken cancellationToken,
        int page = ApiPagination.DefaultPage,
        int pageSize = ApiPagination.DefaultPageSize)
    {
        if (!ApiPagination.IsValid(page, pageSize))
        {
            return Results.BadRequest(ApiPagination.CreateInvalidPaginationProblemDetails());
        }

        var result = await handler.HandleAsync(new ListPartsQuery(page, pageSize), cancellationToken);
        var response = new PagedPartResponse(
            result.Items.Select(MapToResponse).ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount);

        return Results.Ok(response);
    }

    private static async Task<IResult> UpdatePart(
        Guid id,
        UpdatePartRequest request,
        UpdatePartHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new UpdatePartCommand(id, request.Name, request.UnitOfMeasure, request.UnitPrice);
        var dto = await handler.HandleAsync(command, cancellationToken);
        return Results.Ok(MapToResponse(dto));
    }

    private static async Task<IResult> DeactivatePart(
        Guid id,
        DeactivatePartHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new DeactivatePartCommand(id), cancellationToken);
        return Results.NoContent();
    }

    private static PartResponse MapToResponse(Application.Parts.DTOs.PartDto dto) => new(
        dto.Id,
        dto.Name,
        dto.Code,
        dto.Sku,
        dto.UnitOfMeasure,
        dto.UnitPrice,
        dto.IsActive,
        dto.CreatedAt,
        dto.UpdatedAt);
}
