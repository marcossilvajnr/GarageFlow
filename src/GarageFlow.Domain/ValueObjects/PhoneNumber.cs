using GarageFlow.Domain.Customers;
using GarageFlow.Domain.Exceptions;

namespace GarageFlow.Domain.ValueObjects;

public sealed record PhoneNumber
{
    public string Value { get; }

    private PhoneNumber(string value) => Value = value;

    public static PhoneNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(CustomersErrorMessages.InvalidPhoneNumber);

        var digits = new string(value.Where(char.IsDigit).ToArray());

        if (digits.Length < 10 || digits.Length > 11)
            throw new DomainException(CustomersErrorMessages.InvalidPhoneNumber);

        var ddd = int.Parse(digits[..2]);
        if (ddd < 11 || ddd > 99)
            throw new DomainException(CustomersErrorMessages.InvalidPhoneNumber);

        var firstDigit = digits[2] - '0';
        if (digits.Length == 11 && firstDigit != 9)
            throw new DomainException(CustomersErrorMessages.InvalidPhoneNumber);

        if (digits.Length == 10 && (firstDigit < 2 || firstDigit > 8))
            throw new DomainException(CustomersErrorMessages.InvalidPhoneNumber);

        return new PhoneNumber(digits);
    }
}
