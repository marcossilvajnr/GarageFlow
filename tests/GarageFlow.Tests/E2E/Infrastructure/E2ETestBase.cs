using System.Text.Json;
using System.Net;
using System.Net.Http.Json;

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

    internal static async Task ResetRealDatabaseAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/dev/database/reset", new { confirm = true });
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Falha ao resetar banco real para E2E. Status={(int)response.StatusCode}, body={body}");
        }
    }
}
