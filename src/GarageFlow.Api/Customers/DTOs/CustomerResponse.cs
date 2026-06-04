using GarageFlow.Domain.Customers;

namespace GarageFlow.Api.Customers.DTOs;

public sealed record CustomerResponse(
    Guid Id,
    string Name,
    CustomerDocumentType DocumentType,
    string Document,
    string Email,
    string PhoneNumber,
    AddressResponse Address,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
