using GarageFlow.Application.Employees.Commands;
using GarageFlow.Application.Employees.DTOs;
using GarageFlow.Domain.Employees;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.ValueObjects;

namespace GarageFlow.Application.Employees.Handlers;

public sealed class UpdateEmployeeHandler(IEmployeeRepository repository)
{
    public async Task<EmployeeDto> HandleAsync(UpdateEmployeeCommand command, CancellationToken cancellationToken = default)
    {
        var employee = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (employee is null)
            throw new EntityNotFoundException(DomainErrorMessages.EmployeeNotFound(command.Id));

        var address = Address.Create(
            command.Street, command.Number, command.Complement,
            command.Neighborhood, command.City, command.State, command.ZipCode);

        employee.Update(command.Name, command.Email, command.PhoneNumber, address, command.Role);

        repository.Update(employee);
        await repository.SaveChangesAsync(cancellationToken);

        return EmployeeMapper.ToDto(employee);
    }
}