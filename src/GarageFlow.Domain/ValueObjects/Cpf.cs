using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Domain.ValueObjects;

public sealed record Cpf
{
    public string Value { get; }

    private Cpf(string value) => Value = value;

    public static Cpf Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrorMessages.InvalidCpf);

        var digits = new string(value.Where(char.IsDigit).ToArray());

        if (digits.Length != 11)
            throw new DomainException(DomainErrorMessages.InvalidCpf);

        if (digits.Distinct().Count() == 1)
            throw new DomainException(DomainErrorMessages.InvalidCpf);

        if (!ValidateDigit(digits, 10) || !ValidateDigit(digits, 11))
            throw new DomainException(DomainErrorMessages.InvalidCpf);

        return new Cpf(digits);
    }

    private static bool ValidateDigit(string digits, int position)
    {
        var sum = 0;
        for (var i = 0; i < position - 1; i++)
            sum += (digits[i] - '0') * (position - i);

        var remainder = sum % 11;
        var expected = remainder < 2 ? 0 : 11 - remainder;
        return (digits[position - 1] - '0') == expected;
    }
}
