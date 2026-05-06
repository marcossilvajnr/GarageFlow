using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using GarageFlow.Api.DTOs.Auth;
using GarageFlow.Tests.Integration;
using Microsoft.AspNetCore.Mvc;

namespace GarageFlow.Tests.Integration.Auth;

public sealed class AuthEndpointsTests(GarageFlowWebApplicationFactory factory)
    : IClassFixture<GarageFlowWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task Login_WithValidCredentials_Returns200AndBearerToken()
    {
        var response = await _client.PostAsJsonAsync("/auth/login", new LoginRequest("admin", "admin123"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions);

        body.Should().NotBeNull();
        body!.TokenType.Should().Be("Bearer");
        body.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.ExpiresIn.Should().BeGreaterThan(0);
        body.Role.Should().Be("Administrative");
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/auth/login", new LoginRequest("admin", "wrong-password"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithInvalidPayload_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/auth/login", new LoginRequest("", ""));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>(JsonOptions);
        body.Should().NotBeNull();
        body!.Status.Should().Be(400);
        body.Detail.Should().Be("Usuário e senha são obrigatórios");
    }
}
