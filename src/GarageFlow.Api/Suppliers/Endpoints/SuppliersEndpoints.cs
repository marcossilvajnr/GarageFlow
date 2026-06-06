using GarageFlow.Api.Common.Pagination;
using GarageFlow.Api.Suppliers.DTOs;
using GarageFlow.Application.Suppliers.Commands;
using GarageFlow.Application.Suppliers.Handlers;
using GarageFlow.Application.Suppliers.Queries;
using Microsoft.AspNetCore.Mvc;

namespace GarageFlow.Api.Suppliers.Endpoints;

public static class SuppliersEndpoints
{
    public static IEndpointRouteBuilder MapSupplierEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/suppliers")
            .WithTags("Suppliers")
            .RequireAuthorization("Administrative");

        group.MapPost("/", CreateSupplier)
            .WithName("CreateSupplier")
            .WithSummary("Cria um novo fornecedor.")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<SupplierResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapGet("/{id:guid}", GetSupplierById)
            .WithName("GetSupplierById")
            .WithSummary("Consulta fornecedor por Id.")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<SupplierResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", ListSuppliers)
            .WithName("ListSuppliers")
            .WithSummary("Lista fornecedores com paginação.")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<PagedSupplierResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", UpdateSupplier)
            .WithName("UpdateSupplier")
            .WithSummary("Atualiza dados do fornecedor.")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<SupplierResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}", DeactivateSupplier)
            .WithName("DeactivateSupplier")
            .WithSummary("Desativa fornecedor (soft delete).")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        return endpoints;
    }

    private static async Task<IResult> CreateSupplier(
        CreateSupplierRequest request,
        CreateSupplierHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new CreateSupplierCommand(
            request.Name, request.Cnpj, request.Email, request.PhoneNumber,
            request.Street, request.Number, request.Complement,
            request.Neighborhood, request.City, request.State, request.ZipCode);

        var dto = await handler.HandleAsync(command, cancellationToken);

        var response = MapToResponse(dto);
        return Results.Created($"/suppliers/{dto.Id}", response);
    }

    private static async Task<IResult> GetSupplierById(
        Guid id,
        GetSupplierByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var dto = await handler.HandleAsync(new GetSupplierByIdQuery(id), cancellationToken);
        return dto is null ? Results.NotFound() : Results.Ok(MapToResponse(dto));
    }

    private static async Task<IResult> ListSuppliers(
        ListSuppliersHandler handler,
        CancellationToken cancellationToken,
        int page = ApiPagination.DefaultPage,
        int pageSize = ApiPagination.DefaultPageSize)
    {
        if (!ApiPagination.IsValid(page, pageSize))
        {
            return Results.BadRequest(ApiPagination.CreateInvalidPaginationProblemDetails());
        }

        var result = await handler.HandleAsync(new ListSuppliersQuery(page, pageSize), cancellationToken);
        var response = new PagedSupplierResponse(
            result.Items.Select(MapToResponse).ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount);

        return Results.Ok(response);
    }

    private static async Task<IResult> UpdateSupplier(
        Guid id,
        UpdateSupplierRequest request,
        UpdateSupplierHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSupplierCommand(
            id, request.Name, request.Email, request.PhoneNumber,
            request.Street, request.Number, request.Complement,
            request.Neighborhood, request.City, request.State, request.ZipCode);

        var dto = await handler.HandleAsync(command, cancellationToken);
        return Results.Ok(MapToResponse(dto));
    }

    private static async Task<IResult> DeactivateSupplier(
        Guid id,
        DeactivateSupplierHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new DeactivateSupplierCommand(id), cancellationToken);
        return Results.NoContent();
    }

    private static SupplierResponse MapToResponse(Application.Suppliers.DTOs.SupplierDto dto) => new(
        dto.Id, dto.Name, dto.Cnpj, dto.Email, dto.PhoneNumber,
        new AddressResponse(
            dto.Address.Street, dto.Address.Number, dto.Address.Complement,
            dto.Address.Neighborhood, dto.Address.City, dto.Address.State, dto.Address.ZipCode),
        dto.IsActive, dto.CreatedAt, dto.UpdatedAt);
}
