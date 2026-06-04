using GarageFlow.Api.Suppliers.DTOs;
using GarageFlow.Application.Suppliers.Commands;
using GarageFlow.Application.Suppliers.Handlers;
using GarageFlow.Application.Suppliers.Queries;
using GarageFlow.Domain.Exceptions;
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
            .Produces(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}", DeactivateSupplier)
            .WithName("DeactivateSupplier")
            .WithSummary("Desativa fornecedor (soft delete).")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        return endpoints;
    }

    private static async Task<IResult> CreateSupplier(
        CreateSupplierRequest request,
        CreateSupplierHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateSupplierCommand(
                request.Name, request.Cnpj, request.Email, request.PhoneNumber,
                request.Street, request.Number, request.Complement,
                request.Neighborhood, request.City, request.State, request.ZipCode);

            var dto = await handler.HandleAsync(command, cancellationToken);

            var response = MapToResponse(dto);
            return Results.Created($"/suppliers/{dto.Id}", response);
        }
        catch (DuplicateSupplierDataException ex)
        {
            return Results.Conflict(new ProblemDetails { Title = "Conflito", Detail = ex.Message, Status = 409 });
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(new ProblemDetails { Title = "Erro de validação", Detail = ex.Message, Status = 400 });
        }
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
        int page = 1,
        int pageSize = 20)
    {
        if (page < 1 || pageSize < 1)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Erro de validação",
                Detail = "Página e tamanho da página devem ser maiores que zero.",
                Status = 400
            });
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
        try
        {
            var command = new UpdateSupplierCommand(
                id, request.Name, request.Email, request.PhoneNumber,
                request.Street, request.Number, request.Complement,
                request.Neighborhood, request.City, request.State, request.ZipCode);

            var dto = await handler.HandleAsync(command, cancellationToken);
            return Results.Ok(MapToResponse(dto));
        }
        catch (DuplicateSupplierDataException ex)
        {
            return Results.Conflict(new ProblemDetails { Title = "Conflito", Detail = ex.Message, Status = 409 });
        }
        catch (EntityNotFoundException)
        {
            return Results.NotFound();
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(new ProblemDetails { Title = "Erro de validação", Detail = ex.Message, Status = 400 });
        }
    }

    private static async Task<IResult> DeactivateSupplier(
        Guid id,
        DeactivateSupplierHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            await handler.HandleAsync(new DeactivateSupplierCommand(id), cancellationToken);
            return Results.NoContent();
        }
        catch (EntityNotFoundException)
        {
            return Results.NotFound();
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(new ProblemDetails { Title = "Erro de validação", Detail = ex.Message, Status = 400 });
        }
    }

    private static SupplierResponse MapToResponse(Application.Suppliers.DTOs.SupplierDto dto) => new(
        dto.Id, dto.Name, dto.Cnpj, dto.Email, dto.PhoneNumber,
        new AddressResponse(
            dto.Address.Street, dto.Address.Number, dto.Address.Complement,
            dto.Address.Neighborhood, dto.Address.City, dto.Address.State, dto.Address.ZipCode),
        dto.IsActive, dto.CreatedAt, dto.UpdatedAt);
}
