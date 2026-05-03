using GarageFlow.Domain.Customers;
using GarageFlow.Domain.Exceptions;

namespace GarageFlow.Domain.ValueObjects;

public sealed record Address
{
    private static readonly HashSet<string> ValidStates =
    [
        "AC","AL","AP","AM","BA","CE","DF","ES","GO","MA","MT","MS","MG",
        "PA","PB","PR","PE","PI","RJ","RN","RS","RO","RR","SC","SP","SE","TO"
    ];

    public string Street { get; }
    public string Number { get; }
    public string? Complement { get; }
    public string Neighborhood { get; }
    public string City { get; }
    public string State { get; }
    public string ZipCode { get; }

    private Address(string street, string number, string? complement,
        string neighborhood, string city, string state, string zipCode)
    {
        Street = street;
        Number = number;
        Complement = complement;
        Neighborhood = neighborhood;
        City = city;
        State = state;
        ZipCode = zipCode;
    }

    public static Address Create(string street, string number, string? complement,
        string neighborhood, string city, string state, string zipCode)
    {
        if (string.IsNullOrWhiteSpace(street) || street.Trim().Length > 200)
            throw new DomainException(CustomersErrorMessages.InvalidStreet);

        if (string.IsNullOrWhiteSpace(number) || number.Trim().Length > 10)
            throw new DomainException(CustomersErrorMessages.InvalidNumber);

        if (complement is not null && complement.Trim().Length > 100)
            throw new DomainException(CustomersErrorMessages.InvalidComplement);

        if (string.IsNullOrWhiteSpace(neighborhood) || neighborhood.Trim().Length > 100)
            throw new DomainException(CustomersErrorMessages.InvalidNeighborhood);

        if (string.IsNullOrWhiteSpace(city) || city.Trim().Length > 100)
            throw new DomainException(CustomersErrorMessages.InvalidCity);

        var normalizedState = string.IsNullOrWhiteSpace(state) ? "" : state.Trim().ToUpperInvariant();
        if (!ValidStates.Contains(normalizedState))
            throw new DomainException(CustomersErrorMessages.InvalidState);

        var zip = new string((zipCode ?? "").Where(char.IsDigit).ToArray());
        if (zip.Length != 8)
            throw new DomainException(CustomersErrorMessages.InvalidZipCode);

        return new Address(
            street.Trim(), number.Trim(),
            string.IsNullOrWhiteSpace(complement) ? null : complement.Trim(),
            neighborhood.Trim(), city.Trim(), normalizedState, zip);
    }
}
