using System.Text.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GarageFlow.Api.DTOs.Auth;

namespace GarageFlow.Tests.E2E.Infrastructure;

public abstract class E2ETestBase
{
    internal enum E2ERole
    {
        Administrative,
        FrontDesk,
        Mechanic,
        Stockist
    }

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

    internal static async Task AuthenticateAsAsync(HttpClient client, E2ERole role)
    {
        var (username, password) = role switch
        {
            E2ERole.Administrative => ("admin", "admin123"),
            E2ERole.FrontDesk => ("frontdesk", "frontdesk123"),
            E2ERole.Mechanic => ("mechanic", "mechanic123"),
            E2ERole.Stockist => ("stockist", "stockist123"),
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
        };

        var loginResponse = await client.PostAsJsonAsync("/auth/login", new LoginRequest(username, password));
        if (loginResponse.StatusCode != HttpStatusCode.OK)
        {
            var body = await loginResponse.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Falha no login E2E. Status={(int)loginResponse.StatusCode}, role={role}, body={body}");
        }

        var payload = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions)
            ?? throw new InvalidOperationException($"Falha ao desserializar resposta de login para role={role}.");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", payload.AccessToken);
    }

    internal static void ClearAuthentication(HttpClient client)
        => client.DefaultRequestHeaders.Authorization = null;
}
