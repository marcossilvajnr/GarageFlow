using System.Net.Http.Headers;
using GarageFlow.Tests.Integration;

namespace GarageFlow.Tests.E2E.Infrastructure;

public sealed class E2ETestFixture : IClassFixture<E2ERealDbWebApplicationFactory>
{
    private readonly E2ERealDbWebApplicationFactory _factory;

    public E2ETestFixture(E2ERealDbWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public HttpClient CreateClient() => _factory.CreateClient();

    public HttpClient CreateClientWithRole(string role, string subject = "e2e-user")
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Remove(TestAuthHandler.RoleHeader);
        client.DefaultRequestHeaders.Remove(TestAuthHandler.SubHeader);
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
        client.DefaultRequestHeaders.Add(TestAuthHandler.SubHeader, subject);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }
}
