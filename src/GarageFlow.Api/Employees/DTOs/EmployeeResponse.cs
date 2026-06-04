using GarageFlow.Domain.Employees;

namespace GarageFlow.Api.Employees.DTOs;

public sealed record EmployeeResponse(
    Guid Id,
    string Name,
    GarageFlow.Domain.Customers.CustomerDocumentType DocumentType,
    string Document,
    string Email,
    string PhoneNumber,
    AddressResponse Address,
    EmployeeRole Role,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);