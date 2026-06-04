using GarageFlow.Application.Employees.Commands;
using GarageFlow.Application.Employees.DTOs;
using GarageFlow.Application.Employees.Mappers;
using GarageFlow.Domain.Employees;
using GarageFlow.Domain.ValueObjects;

namespace GarageFlow.Application.Employees.Handlers;

public sealed class CreateEmployeeHandler(IEmployeeRepository repository)
{
    public async Task<EmployeeDto> HandleAsync(CreateEmployeeCommand command, CancellationToken cancellationToken = default)
    {
        var address = Address.Create(
            command.Street, command.Number, command.Complement,
            command.Neighborhood, command.City, command.State, command.ZipCode);

        var employee = Employee.Create(
            command.Name,
            EmployeeDocumentTypeMapper.ToDomain(command.DocumentType),
            command.Document,
            command.Email,
            command.PhoneNumber,
            address,
            EmployeeRoleMapper.ToDomain(command.Role));

        await repository.AddAsync(employee, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return EmployeeMapper.ToDto(employee);
    }
}
