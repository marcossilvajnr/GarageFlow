namespace GarageFlow.Api.Suppliers.DTOs;

public sealed record UpdateSupplierRequest(
    string Name,
    string Email,
    string PhoneNumber,
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    string ZipCode);
