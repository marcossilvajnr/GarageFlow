namespace GarageFlow.Application.Suppliers.Commands;

public sealed record CreateSupplierCommand(
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
