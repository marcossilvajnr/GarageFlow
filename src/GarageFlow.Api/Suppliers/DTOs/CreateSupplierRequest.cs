namespace GarageFlow.Api.Suppliers.DTOs;

public sealed record CreateSupplierRequest(
    string Name,
    string Cnpj,
    string Email,
    string PhoneNumber,
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    string ZipCode);
