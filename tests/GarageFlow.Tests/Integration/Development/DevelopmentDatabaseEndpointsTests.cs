using GarageFlow.Api.Common.Authorization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace GarageFlow.Tests.Integration.Development;

public sealed class DevelopmentDatabaseEndpointsTests(GarageFlowWebApplicationFactory factory)
    : IClassFixture<GarageFlowWebApplicationFactory>
{
    private const string DestructiveOperationBlockedDetail =
        "Operacao destrutiva bloqueada. Envie { \"confirm\": true } para prosseguir.";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private HttpClient CreateClientWithRole(string role)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
        return client;
    }

    private HttpClient CreateAnonymousClient()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.ForceAnonymousHeader, bool.TrueString);
        return client;
    }

    [Theory]
    [InlineData("/dev/database/clean")]
    [InlineData("/dev/database/reset")]
    public async Task DestructiveDatabaseEndpoint_WithoutConfirmation_ReturnsValidationProblemDetails(string url)
    {
        var client = CreateClientWithRole(ApiRoles.Administrative);

        var response = await client.PostAsJsonAsync(url, new { confirm = false });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>(JsonOptions);
        body.Should().NotBeNull();
        body!.Status.Should().Be(400);
        body.Title.Should().Be("Erro de validação");
        body.Detail.Should().Be(DestructiveOperationBlockedDetail);
    }

    [Theory]
    [InlineData("/dev/database/migrate")]
    [InlineData("/dev/database/clean")]
    [InlineData("/dev/database/reset")]
    public async Task DevelopmentDatabaseEndpoint_WithoutAuthentication_Returns401(string url)
    {
        var client = CreateAnonymousClient();

        var response = await client.PostAsJsonAsync(url, new { confirm = false });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("/dev/database/migrate")]
    [InlineData("/dev/database/clean")]
    [InlineData("/dev/database/reset")]
    public async Task DevelopmentDatabaseEndpoint_WithNonAdministrativeRole_Returns403(string url)
    {
        var client = CreateClientWithRole(ApiRoles.FrontDesk);

        var response = await client.PostAsJsonAsync(url, new { confirm = false });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
