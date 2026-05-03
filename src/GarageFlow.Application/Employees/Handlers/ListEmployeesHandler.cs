using GarageFlow.Application.Employees.DTOs;
using GarageFlow.Application.Employees.Queries;
using GarageFlow.Domain.Employees;

namespace GarageFlow.Application.Employees.Handlers;

public sealed class ListEmployeesHandler(IEmployeeRepository repository)
{
    public async Task<PagedResult<EmployeeDto>> HandleAsync(ListEmployeesQuery query, CancellationToken cancellationToken = default)
    {
        var (employees, totalCount) = await repository.ListAsync(query.Page, query.PageSize, cancellationToken);

        var employeeDtos = employees.Select(EmployeeMapper.ToDto).ToList();

        return new PagedResult<EmployeeDto>(employeeDtos, query.Page, query.PageSize, totalCount);
    }
}