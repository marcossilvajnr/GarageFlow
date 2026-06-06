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

    private readonly HttpClient _client = factory.CreateClient();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Theory]
    [InlineData("/dev/database/clean")]
    [InlineData("/dev/database/reset")]
    public async Task DestructiveDatabaseEndpoint_WithoutConfirmation_ReturnsValidationProblemDetails(string url)
    {
        var response = await _client.PostAsJsonAsync(url, new { confirm = false });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>(JsonOptions);
        body.Should().NotBeNull();
        body!.Status.Should().Be(400);
        body.Title.Should().Be("Erro de validação");
        body.Detail.Should().Be(DestructiveOperationBlockedDetail);
    }
}
