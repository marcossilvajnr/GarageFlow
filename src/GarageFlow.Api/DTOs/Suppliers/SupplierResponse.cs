namespace GarageFlow.Api.DTOs.Suppliers;

public sealed record SupplierResponse(
    Guid Id,
    string Name,
    string Cnpj,
    string Email,
    string PhoneNumber,
    AddressResponse Address,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
