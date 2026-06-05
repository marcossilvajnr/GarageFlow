using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using GarageFlow.Api.Auth.DTOs;
using GarageFlow.Application.Auth.DTOs;
using GarageFlow.Application.Auth.Interfaces;
using GarageFlow.Tests.Integration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GarageFlow.Tests.Integration.ErrorHandling;

public sealed class ErrorHandlingProblemDetailsTests(GarageFlowWebApplicationFactory factory)
    : IClassFixture<GarageFlowWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task ProblemDetails_ForHandledException_IncludesTraceAndCorrelationIds()
    {
        const string correlationId = "test-correlation-id";
        var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/login")
        {
            Content = JsonContent.Create(new LoginRequest("", ""))
        };
        request.Headers.Add("X-Correlation-ID", correlationId);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>(JsonOptions);

        body.Should().NotBeNull();
        body!.Extensions.Should().ContainKey("traceId");
        body.Extensions["traceId"]?.ToString().Should().NotBeNullOrWhiteSpace();
        body.Extensions.Should().ContainKey("correlationId");
        body.Extensions["correlationId"]?.ToString().Should().Be(correlationId);
    }

    [Fact]
    public async Task ProblemDetails_ForUnhandledException_DoesNotExposeExceptionMessage()
    {
        var client = CreateClientWithThrowingCredentialStore();

        var response = await client.PostAsJsonAsync("/auth/login", new LoginRequest("admin", "admin123"));

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>(JsonOptions);

        body.Should().NotBeNull();
        body!.Status.Should().Be(500);
        body.Title.Should().Be("Erro interno");
        body.Detail.Should().Be("Ocorreu um erro inesperado.");
        body.Detail.Should().NotBe(ThrowingAuthUserCredentialStore.ExceptionMessage);
    }

    private HttpClient CreateClientWithThrowingCredentialStore()
    {
        var throwingFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IAuthUserCredentialStore>();
                services.AddScoped<IAuthUserCredentialStore, ThrowingAuthUserCredentialStore>();
            });
        });

        return throwingFactory.CreateClient();
    }

    private sealed class ThrowingAuthUserCredentialStore : IAuthUserCredentialStore
    {
        internal const string ExceptionMessage = "database password leaked";

        public Task<AuthUserCredentialDto?> GetByUsernameAsync(
            string username,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(ExceptionMessage);
        }
    }
}
