using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using GarageFlow.Infrastructure.Persistence;
using Npgsql;
using System.IO;

namespace GarageFlow.Tests.E2E.Infrastructure;

public sealed class E2ERealDbWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string RealDbConnectionEnvVar = "E2E_REAL_DB_CONNECTION";
    private const string PostgresHostEnvVar = "POSTGRES_HOST";
    private const string PostgresPortEnvVar = "POSTGRES_PORT";
    private const string PostgresDbEnvVar = "POSTGRES_DB";
    private const string PostgresUserEnvVar = "POSTGRES_USER";
    private const string PostgresPasswordEnvVar = "POSTGRES_PASSWORD";
    private const string E2EDatabaseName = "garageflow_e2e";

    private static readonly object DatabaseInitLock = new();
    private static readonly HashSet<string> InitializedDatabases = [];
    private static readonly IReadOnlyDictionary<string, string> DotEnvValues = LoadDotEnvValues();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var connectionString = ResolveConnectionString();

        EnsureDatabaseExists(connectionString);

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:GarageFlow"] = connectionString,
                ["Database:AutoMigrateOnStartup"] = "true"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<GarageFlowDbContext>));
            services.RemoveAll(typeof(DbContextOptions));
            services.RemoveAll(typeof(GarageFlowDbContext));
            services.RemoveAll(typeof(IDbContextOptionsConfiguration<GarageFlowDbContext>));

            services.AddDbContext<GarageFlowDbContext>(options =>
                options.UseNpgsql(connectionString));
        });

        builder.UseEnvironment("Development");
    }

    private static string ResolveConnectionString()
    {
        var explicitConnection = GetSetting(RealDbConnectionEnvVar);
        if (!string.IsNullOrWhiteSpace(explicitConnection))
            return explicitConnection;

        var host = GetSetting(PostgresHostEnvVar) ?? "localhost";
        var port = GetSetting(PostgresPortEnvVar) ?? "5432";
        var baseDatabase = GetSetting(PostgresDbEnvVar) ?? "garageflow";
        var username = GetRequiredSetting(PostgresUserEnvVar);
        var password = GetRequiredSetting(PostgresPasswordEnvVar);

        var e2eDatabase = $"{baseDatabase}_e2e";
        if (string.Equals(baseDatabase, E2EDatabaseName, StringComparison.OrdinalIgnoreCase))
            e2eDatabase = baseDatabase;

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = int.TryParse(port, out var parsedPort) ? parsedPort : 5432,
            Database = e2eDatabase,
            Username = username,
            Password = password
        };

        return builder.ConnectionString;
    }

    private static string? GetSetting(string key)
    {
        var envValue = Environment.GetEnvironmentVariable(key);
        if (!string.IsNullOrWhiteSpace(envValue))
            return envValue;

        if (DotEnvValues.TryGetValue(key, out var dotEnvValue) && !string.IsNullOrWhiteSpace(dotEnvValue))
            return dotEnvValue;

        return null;
    }

    private static string GetRequiredSetting(string key)
        => GetSetting(key) ?? throw new InvalidOperationException(
            $"Variável obrigatória ausente para E2E real DB: {key}");

    private static void EnsureDatabaseExists(string connectionString)
    {
        var targetBuilder = new NpgsqlConnectionStringBuilder(connectionString);
        var databaseName = targetBuilder.Database;
        if (string.IsNullOrWhiteSpace(databaseName))
            return;

        var databaseKey = $"{targetBuilder.Host}:{targetBuilder.Port}/{databaseName}";
        lock (DatabaseInitLock)
        {
            if (InitializedDatabases.Contains(databaseKey))
                return;
        }

        var adminBuilder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            Database = "postgres",
            Pooling = false
        };

        using (var connection = new NpgsqlConnection(adminBuilder.ConnectionString))
        {
            connection.Open();

            using var existsCommand = connection.CreateCommand();
            existsCommand.CommandText = "SELECT 1 FROM pg_database WHERE datname = @db";
            existsCommand.Parameters.AddWithValue("db", databaseName);

            var exists = existsCommand.ExecuteScalar() is not null;
            if (!exists)
            {
                using var createCommand = connection.CreateCommand();
                createCommand.CommandText =
                    $"CREATE DATABASE \"{databaseName.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
                createCommand.ExecuteNonQuery();
            }
        }

        lock (DatabaseInitLock)
        {
            InitializedDatabases.Add(databaseKey);
        }
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
