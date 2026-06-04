using GarageFlow.Api.Customers.DTOs;
using GarageFlow.Application.Customers.Commands;
using GarageFlow.Application.Customers.Handlers;
using GarageFlow.Application.Customers.Queries;
using GarageFlow.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace GarageFlow.Api.Customers.Endpoints;

public static class CustomersEndpoints
{
    public static IEndpointRouteBuilder MapCustomerEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/customers")
            .WithTags("Customers")
            .RequireAuthorization("Administrative");

        group.MapPost("/", CreateCustomer)
            .WithName("CreateCustomer")
            .WithSummary("Cria um novo cliente.")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<CustomerResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapGet("/{id:guid}", GetCustomerById)
            .WithName("GetCustomerById")
            .WithSummary("Consulta cliente por Id.")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<CustomerResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", ListCustomers)
            .WithName("ListCustomers")
            .WithSummary("Lista clientes com paginação.")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<PagedCustomerResponse>(StatusCodes.Status200OK);

        group.MapPut("/{id:guid}", UpdateCustomer)
            .WithName("UpdateCustomer")
            .WithSummary("Atualiza dados do cliente.")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<CustomerResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeactivateCustomer)
            .WithName("DeactivateCustomer")
            .WithSummary("Desativa cliente (soft delete).")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static async Task<IResult> CreateCustomer(
        CreateCustomerRequest request,
        CreateCustomerHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateCustomerCommand(
                request.Name, request.DocumentType, request.Document,
                request.Email, request.PhoneNumber,
                request.Street, request.Number, request.Complement,
                request.Neighborhood, request.City, request.State, request.ZipCode);

            var dto = await handler.HandleAsync(command, cancellationToken);

            var response = MapToResponse(dto);
            return Results.Created($"/customers/{dto.Id}", response);
        }
        catch (DuplicateDocumentException ex)
        {
            return Results.Conflict(new ProblemDetails { Title = "Conflito", Detail = ex.Message, Status = 409 });
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(new ProblemDetails { Title = "Erro de validação", Detail = ex.Message, Status = 400 });
        }
    }

    private static async Task<IResult> GetCustomerById(
        Guid id,
        GetCustomerByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var dto = await handler.HandleAsync(new GetCustomerByIdQuery(id), cancellationToken);
        return dto is null ? Results.NotFound() : Results.Ok(MapToResponse(dto));
    }

    private static async Task<IResult> ListCustomers(
        ListCustomersHandler handler,
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

        var result = await handler.HandleAsync(new ListCustomersQuery(page, pageSize), cancellationToken);
        var response = new PagedCustomerResponse(
            result.Items.Select(MapToResponse).ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount);

        return Results.Ok(response);
    }

    private static async Task<IResult> UpdateCustomer(
        Guid id,
        UpdateCustomerRequest request,
        UpdateCustomerHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateCustomerCommand(
                id, request.Name, request.Email, request.PhoneNumber,
                request.Street, request.Number, request.Complement,
                request.Neighborhood, request.City, request.State, request.ZipCode);

            var dto = await handler.HandleAsync(command, cancellationToken);
            return Results.Ok(MapToResponse(dto));
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

    private static async Task<IResult> DeactivateCustomer(
        Guid id,
        DeactivateCustomerHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            await handler.HandleAsync(new DeactivateCustomerCommand(id), cancellationToken);
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

    private static CustomerResponse MapToResponse(Application.Customers.DTOs.CustomerDto dto) => new(
        dto.Id, dto.Name, dto.DocumentType, dto.Document, dto.Email, dto.PhoneNumber,
        new AddressResponse(
            dto.Address.Street, dto.Address.Number, dto.Address.Complement,
            dto.Address.Neighborhood, dto.Address.City, dto.Address.State, dto.Address.ZipCode),
        dto.IsActive, dto.CreatedAt, dto.UpdatedAt);


}
