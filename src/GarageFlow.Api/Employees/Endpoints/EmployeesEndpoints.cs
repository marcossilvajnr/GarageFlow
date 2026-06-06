using GarageFlow.Api.Common.Pagination;
using GarageFlow.Api.Employees.DTOs;
using GarageFlow.Application.Employees.Commands;
using GarageFlow.Application.Employees.Handlers;
using GarageFlow.Application.Employees.Queries;
using Microsoft.AspNetCore.Mvc;

namespace GarageFlow.Api.Employees.Endpoints;

public static class EmployeesEndpoints
{
    public static IEndpointRouteBuilder MapEmployeeEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/employees")
            .WithTags("Employees")
            .RequireAuthorization("Administrative");

        group.MapPost("/", CreateEmployee)
            .WithName("CreateEmployee")
            .WithSummary("Cria um novo funcionário.")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<EmployeeResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapGet("/{id:guid}", GetEmployeeById)
            .WithName("GetEmployeeById")
            .WithSummary("Consulta funcionário por Id.")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<EmployeeResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", ListEmployees)
            .WithName("ListEmployees")
            .WithSummary("Lista funcionários com paginação.")
            .Produces<PagedEmployeeResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPut("/{id:guid}", UpdateEmployee)
            .WithName("UpdateEmployee")
            .WithSummary("Atualiza dados do funcionário.")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<EmployeeResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeactivateEmployee)
            .WithName("DeactivateEmployee")
            .WithSummary("Desativa funcionário (soft delete).")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        return endpoints;
    }

    private static async Task<IResult> CreateEmployee(
        CreateEmployeeRequest request,
        CreateEmployeeHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new CreateEmployeeCommand(
            request.Name, request.DocumentType, request.Document,
            request.Email, request.PhoneNumber,
            request.Street, request.Number, request.Complement,
            request.Neighborhood, request.City, request.State, request.ZipCode,
            request.Role);

        var dto = await handler.HandleAsync(command, cancellationToken);

        var response = MapToResponse(dto);
        return Results.Created($"/employees/{dto.Id}", response);
    }

    private static async Task<IResult> GetEmployeeById(
        Guid id,
        GetEmployeeByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var dto = await handler.HandleAsync(new GetEmployeeByIdQuery(id), cancellationToken);
        return dto is null ? Results.NotFound() : Results.Ok(MapToResponse(dto));
    }

    private static async Task<IResult> ListEmployees(
        ListEmployeesHandler handler,
        CancellationToken cancellationToken,
        int page = ApiPagination.DefaultPage,
        int pageSize = ApiPagination.DefaultPageSize)
    {
        if (!ApiPagination.IsValid(page, pageSize))
        {
            return Results.BadRequest(ApiPagination.CreateInvalidPaginationProblemDetails());
        }

        var result = await handler.HandleAsync(new ListEmployeesQuery(page, pageSize), cancellationToken);
        var response = new PagedEmployeeResponse(
            result.Items.Select(MapToResponse).ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount);

        return Results.Ok(response);
    }

    private static async Task<IResult> UpdateEmployee(
        Guid id,
        UpdateEmployeeRequest request,
        UpdateEmployeeHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new UpdateEmployeeCommand(
            id, request.Name, request.Email, request.PhoneNumber,
            request.Street, request.Number, request.Complement,
            request.Neighborhood, request.City, request.State, request.ZipCode,
            request.Role);

        var dto = await handler.HandleAsync(command, cancellationToken);
        return Results.Ok(MapToResponse(dto));
    }

    private static async Task<IResult> DeactivateEmployee(
        Guid id,
        DeactivateEmployeeHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new DeactivateEmployeeCommand(id), cancellationToken);
        return Results.NoContent();
    }

    private static EmployeeResponse MapToResponse(Application.Employees.DTOs.EmployeeDto dto) => new(
        dto.Id, dto.Name, dto.DocumentType, dto.Document, dto.Email, dto.PhoneNumber,
        new AddressResponse(
            dto.Address.Street, dto.Address.Number, dto.Address.Complement,
            dto.Address.Neighborhood, dto.Address.City, dto.Address.State, dto.Address.ZipCode),
        dto.Role, dto.IsActive, dto.CreatedAt, dto.UpdatedAt);
}
