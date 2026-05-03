using GarageFlow.Application.Employees.DTOs;
using GarageFlow.Application.Employees.Queries;
using GarageFlow.Domain.Employees;

namespace GarageFlow.Application.Employees.Handlers;

public sealed class GetEmployeeByIdHandler(IEmployeeRepository repository)
{
    public async Task<EmployeeDto?> HandleAsync(GetEmployeeByIdQuery query, CancellationToken cancellationToken = default)
    {
        var employee = await repository.GetByIdAsync(query.Id, cancellationToken);
        return employee is null ? null : EmployeeMapper.ToDto(employee);
    }
}