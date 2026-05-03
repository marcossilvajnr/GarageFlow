using GarageFlow.Application.Customers.DTOs;
using GarageFlow.Domain.Employees;

namespace GarageFlow.Application.Employees.DTOs;

public sealed record EmployeeDto(
    Guid Id,
    string Name,
    GarageFlow.Domain.Customers.CustomerDocumentType DocumentType,
    string Document,
    string Email,
    string PhoneNumber,
    AddressDto Address,
    EmployeeRole Role,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);