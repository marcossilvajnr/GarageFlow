using GarageFlow.Application.Employees.DTOs;
using GarageFlow.Application.Employees.Queries;
using GarageFlow.Domain.Employees;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Application.Employees.Handlers;

public sealed class GetEmployeeByIdHandler(IEmployeeRepository repository)
{
    public async Task<EmployeeDto> HandleAsync(GetEmployeeByIdQuery query, CancellationToken cancellationToken = default)
    {
        var employee = await repository.GetByIdAsync(query.Id, cancellationToken)
            ?? throw new EntityNotFoundException(DomainErrorMessages.EmployeeNotFound(query.Id));

        return EmployeeMapper.ToDto(employee);
    }
}
