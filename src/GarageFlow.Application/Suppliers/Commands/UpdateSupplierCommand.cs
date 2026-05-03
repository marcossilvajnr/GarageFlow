namespace GarageFlow.Application.Suppliers.Commands;

public sealed record UpdateSupplierCommand(
    Guid Id,
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
