using GarageFlow.Application.Customers.DTOs;
using GarageFlow.Application.Employees.DTOs;
using GarageFlow.Domain.Employees;

namespace GarageFlow.Application.Employees.Handlers;

internal static class EmployeeMapper
{
    internal static EmployeeDto ToDto(Employee employee) => new(
        employee.Id,
        employee.Name,
        employee.DocumentType,
        employee.DocumentType == Domain.Customers.CustomerDocumentType.Cpf
            ? employee.Cpf!.Value
            : employee.Cnpj!.Value,
        employee.Email.Value,
        employee.PhoneNumber.Value,
        new AddressDto(
            employee.Address.Street,
            employee.Address.Number,
            employee.Address.Complement,
            employee.Address.Neighborhood,
            employee.Address.City,
            employee.Address.State,
            employee.Address.ZipCode),
        employee.Role,
        employee.IsActive,
        employee.CreatedAt,
        employee.UpdatedAt);
}