using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Domain.ValueObjects;

public sealed record Renavam
{
    public string Value { get; }

    private Renavam(string value) => Value = value;

    public static Renavam Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrorMessages.InvalidRenavam);

        // Extract only digits
        var digits = new string(value.Where(char.IsDigit).ToArray());

        // Must have exactly 11 digits
        if (digits.Length != 11)
            throw new DomainException(DomainErrorMessages.InvalidRenavam);

        // Validate check digit
        if (!ValidateCheckDigit(digits))
            throw new DomainException(DomainErrorMessages.InvalidRenavam);

        return new Renavam(digits);
    }

    private static bool ValidateCheckDigit(string digits)
    {
        // RENAVAM check digit algorithm
        // Sequence: 3298765432 (multipliers for each of the first 10 digits)
        var multipliers = new[] { 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        var sum = 0;

        for (int i = 0; i < 10; i++)
        {
            sum += (digits[i] - '0') * multipliers[i];
        }

        var remainder = sum % 11;
        var expectedCheckDigit = remainder == 10 ? 0 : remainder;
        var actualCheckDigit = digits[10] - '0';

        return actualCheckDigit == expectedCheckDigit;
    }
}
