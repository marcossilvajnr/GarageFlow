using GarageFlow.Application.Customers.DTOs;
using GarageFlow.Application.Customers.Enums;
using GarageFlow.Application.Employees.Enums;

namespace GarageFlow.Application.Employees.DTOs;

public sealed record EmployeeDto(
    Guid Id,
    string Name,
    CustomerDocumentType DocumentType,
    string Document,
    string Email,
    string PhoneNumber,
    AddressDto Address,
    EmployeeRole Role,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
