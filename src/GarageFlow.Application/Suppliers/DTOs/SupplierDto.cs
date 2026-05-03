namespace GarageFlow.Application.Suppliers.DTOs;

public sealed record SupplierDto(
    Guid Id,
    string Name,
    string Cnpj,
    string Email,
    string PhoneNumber,
    AddressDto Address,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
