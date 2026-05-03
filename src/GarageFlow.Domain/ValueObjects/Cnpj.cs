using GarageFlow.Domain.Customers;
using GarageFlow.Domain.Exceptions;

namespace GarageFlow.Domain.ValueObjects;

public sealed record Cnpj
{
    public string Value { get; }

    private Cnpj(string value) => Value = value;

    public static Cnpj Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(CustomersErrorMessages.InvalidCnpj);

        var digits = new string(value.Where(char.IsDigit).ToArray());

        if (digits.Length != 14)
            throw new DomainException(CustomersErrorMessages.InvalidCnpj);

        if (digits.Distinct().Count() == 1)
            throw new DomainException(CustomersErrorMessages.InvalidCnpj);

        if (!ValidateDigit(digits, [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2], 12) ||
            !ValidateDigit(digits, [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2], 13))
            throw new DomainException(CustomersErrorMessages.InvalidCnpj);

        return new Cnpj(digits);
    }

    private static bool ValidateDigit(string digits, int[] weights, int position)
    {
        var sum = 0;
        for (var i = 0; i < weights.Length; i++)
            sum += (digits[i] - '0') * weights[i];

        var remainder = sum % 11;
        var expected = remainder < 2 ? 0 : 11 - remainder;
        return (digits[position] - '0') == expected;
    }
}
