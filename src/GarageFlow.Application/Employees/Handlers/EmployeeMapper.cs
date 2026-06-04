using GarageFlow.Application.Customers.DTOs;
using GarageFlow.Application.Employees.DTOs;
using GarageFlow.Application.Employees.Mappers;
using GarageFlow.Domain.Employees;

namespace GarageFlow.Application.Employees.Handlers;

internal static class EmployeeMapper
{
    internal static EmployeeDto ToDto(Employee employee) => new(
        employee.Id,
        employee.Name,
        EmployeeDocumentTypeMapper.ToApplication(employee.DocumentType),
        ResolveDocument(employee),
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
        EmployeeRoleMapper.ToApplication(employee.Role),
        employee.IsActive,
        employee.CreatedAt,
        employee.UpdatedAt);

    private static string ResolveDocument(Employee employee) =>
        employee.DocumentType switch
        {
            Domain.Customers.CustomerDocumentType.Cpf =>
                employee.Cpf?.Value ?? employee.Cnpj?.Value ?? string.Empty,
            Domain.Customers.CustomerDocumentType.Cnpj =>
                employee.Cnpj?.Value ?? employee.Cpf?.Value ?? string.Empty,
            _ => employee.Cpf?.Value ?? employee.Cnpj?.Value ?? string.Empty
        };
}
