using GarageFlow.Domain.Customers;

namespace GarageFlow.Application.Customers.DTOs;

public sealed record CustomerDto(
    Guid Id,
    string Name,
    CustomerDocumentType DocumentType,
    string Document,
    string Email,
    string PhoneNumber,
    AddressDto Address,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
