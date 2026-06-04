using GarageFlow.Application.Customers.Enums;
using GarageFlow.Application.Employees.Enums;

namespace GarageFlow.Api.Employees.DTOs;

public sealed record EmployeeResponse(
    Guid Id,
    string Name,
    CustomerDocumentType DocumentType,
    string Document,
    string Email,
    string PhoneNumber,
    AddressResponse Address,
    EmployeeRole Role,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
