namespace GarageFlow.Api.DTOs.Customers;

public sealed record UpdateCustomerRequest(
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
