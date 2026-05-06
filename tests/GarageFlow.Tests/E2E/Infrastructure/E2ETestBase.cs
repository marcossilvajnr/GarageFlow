using System.Text.Json;

namespace GarageFlow.Tests.E2E.Infrastructure;

public abstract class E2ETestBase
{
    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    internal static string UniqueCode(string prefix, int maxLength = 20)
    {
        var value = $"{prefix}-{Guid.NewGuid():N}";
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
