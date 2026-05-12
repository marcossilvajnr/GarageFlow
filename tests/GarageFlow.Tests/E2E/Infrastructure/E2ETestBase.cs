using System.Text.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.IO;
using GarageFlow.Api.DTOs.Auth;

namespace GarageFlow.Tests.E2E.Infrastructure;

public abstract class E2ETestBase
{
    private static readonly IReadOnlyDictionary<string, string> DotEnvValues = LoadDotEnvValues();

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
            E2ERole.Administrative => (
                GetRequiredSetting(role, "username", "E2E_ADMIN_USERNAME", "API_USERNAME"),
                GetRequiredSetting(role, "password", "E2E_ADMIN_PASSWORD", "API_PASSWORD")),
            E2ERole.FrontDesk => (
                GetRequiredSetting(role, "username", "E2E_FRONTDESK_USERNAME"),
                GetRequiredSetting(role, "password", "E2E_FRONTDESK_PASSWORD")),
            E2ERole.Mechanic => (
                GetRequiredSetting(role, "username", "E2E_MECHANIC_USERNAME"),
                GetRequiredSetting(role, "password", "E2E_MECHANIC_PASSWORD")),
            E2ERole.Stockist => (
                GetRequiredSetting(role, "username", "E2E_STOCKIST_USERNAME"),
                GetRequiredSetting(role, "password", "E2E_STOCKIST_PASSWORD")),
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

    private static string? GetSetting(params string[] keys)
    {
        foreach (var key in keys)
        {
            var envValue = Environment.GetEnvironmentVariable(key);
            if (!string.IsNullOrWhiteSpace(envValue))
                return envValue;

            if (DotEnvValues.TryGetValue(key, out var dotEnvValue) && !string.IsNullOrWhiteSpace(dotEnvValue))
                return dotEnvValue;
        }

        return null;
    }

    private static string GetRequiredSetting(E2ERole role, string kind, params string[] keys)
    {
        var value = GetSetting(keys);
        if (!string.IsNullOrWhiteSpace(value))
            return value;

        throw new InvalidOperationException(
            $"Credencial E2E ausente para role={role} ({kind}). Defina uma das variáveis: {string.Join(", ", keys)}.");
    }

    private static IReadOnlyDictionary<string, string> LoadDotEnvValues()
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var dotEnvPath = FindDotEnvPath();
        if (dotEnvPath is null || !File.Exists(dotEnvPath))
            return values;

        foreach (var rawLine in File.ReadAllLines(dotEnvPath))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
                continue;

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
                continue;

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim();
            if (value.StartsWith('"') && value.EndsWith('"') && value.Length >= 2)
                value = value[1..^1];

            if (key.Length > 0)
                values[key] = value;
        }

        return values;
    }

    private static string? FindDotEnvPath()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, ".env");
            if (File.Exists(candidate))
                return candidate;

            current = current.Parent;
        }

        return null;
    }
}
