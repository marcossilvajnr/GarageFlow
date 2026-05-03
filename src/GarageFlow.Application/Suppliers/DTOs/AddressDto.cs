namespace GarageFlow.Application.Suppliers.DTOs;

public sealed record AddressDto(
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    string ZipCode);
