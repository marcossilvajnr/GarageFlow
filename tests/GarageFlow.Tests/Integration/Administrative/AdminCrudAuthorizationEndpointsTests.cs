using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GarageFlow.Tests.Integration;

namespace GarageFlow.Tests.Integration.Administrative;

public sealed class AdminCrudAuthorizationEndpointsTests(GarageFlowWebApplicationFactory factory)
    : IClassFixture<GarageFlowWebApplicationFactory>
{
    private HttpClient CreateClientWithRole(string role)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
        return client;
    }

    private HttpClient CreateAnonymousClient()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.ForceAnonymousHeader, "true");
        return client;
    }

    [Theory]
    [InlineData("/customers?page=1&pageSize=5")]
    [InlineData("/vehicles?page=1&pageSize=5")]
    [InlineData("/suppliers?page=1&pageSize=5")]
    [InlineData("/employees?page=1&pageSize=5")]
    [InlineData("/services?page=1&pageSize=5")]
    [InlineData("/parts?page=1&pageSize=5")]
    [InlineData("/supplies?page=1&pageSize=5")]
    public async Task ListEndpoints_WithoutToken_Return401(string url)
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync(url);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("/customers?page=1&pageSize=5")]
    [InlineData("/vehicles?page=1&pageSize=5")]
    [InlineData("/suppliers?page=1&pageSize=5")]
    [InlineData("/employees?page=1&pageSize=5")]
    [InlineData("/services?page=1&pageSize=5")]
    [InlineData("/parts?page=1&pageSize=5")]
    [InlineData("/supplies?page=1&pageSize=5")]
    public async Task ListEndpoints_WithNonAdministrativeRole_Return403(string url)
    {
        var client = CreateClientWithRole("FrontDesk");

        var response = await client.GetAsync(url);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Theory]
    [InlineData("/customers?page=1&pageSize=5")]
    [InlineData("/vehicles?page=1&pageSize=5")]
    [InlineData("/suppliers?page=1&pageSize=5")]
    [InlineData("/employees?page=1&pageSize=5")]
    [InlineData("/services?page=1&pageSize=5")]
    [InlineData("/parts?page=1&pageSize=5")]
    [InlineData("/supplies?page=1&pageSize=5")]
    public async Task ListEndpoints_WithAdministrativeRole_Return200(string url)
    {
        var client = CreateClientWithRole("Administrative");

        var response = await client.GetAsync(url);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("/customers", "POST")]
    [InlineData("/vehicles", "POST")]
    [InlineData("/suppliers", "POST")]
    [InlineData("/employees", "POST")]
    [InlineData("/services", "POST")]
    [InlineData("/parts", "POST")]
    [InlineData("/supplies", "POST")]
    [InlineData("/customers/00000000-0000-0000-0000-000000000001", "PUT")]
    [InlineData("/vehicles/00000000-0000-0000-0000-000000000001", "PUT")]
    [InlineData("/suppliers/00000000-0000-0000-0000-000000000001", "PUT")]
    [InlineData("/employees/00000000-0000-0000-0000-000000000001", "PUT")]
    [InlineData("/services/00000000-0000-0000-0000-000000000001", "PUT")]
    [InlineData("/parts/00000000-0000-0000-0000-000000000001", "PUT")]
    [InlineData("/supplies/00000000-0000-0000-0000-000000000001", "PUT")]
    [InlineData("/customers/00000000-0000-0000-0000-000000000001", "DELETE")]
    [InlineData("/vehicles/00000000-0000-0000-0000-000000000001", "DELETE")]
    [InlineData("/suppliers/00000000-0000-0000-0000-000000000001", "DELETE")]
    [InlineData("/employees/00000000-0000-0000-0000-000000000001", "DELETE")]
    [InlineData("/services/00000000-0000-0000-0000-000000000001", "DELETE")]
    [InlineData("/parts/00000000-0000-0000-0000-000000000001", "DELETE")]
    [InlineData("/supplies/00000000-0000-0000-0000-000000000001", "DELETE")]
    public async Task WriteEndpoints_WithoutToken_Return401(string url, string method)
    {
        var client = CreateAnonymousClient();
        var request = CreateWriteRequest(url, method);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("/customers", "POST")]
    [InlineData("/vehicles", "POST")]
    [InlineData("/suppliers", "POST")]
    [InlineData("/employees", "POST")]
    [InlineData("/services", "POST")]
    [InlineData("/parts", "POST")]
    [InlineData("/supplies", "POST")]
    [InlineData("/customers/00000000-0000-0000-0000-000000000001", "PUT")]
    [InlineData("/vehicles/00000000-0000-0000-0000-000000000001", "PUT")]
    [InlineData("/suppliers/00000000-0000-0000-0000-000000000001", "PUT")]
    [InlineData("/employees/00000000-0000-0000-0000-000000000001", "PUT")]
    [InlineData("/services/00000000-0000-0000-0000-000000000001", "PUT")]
    [InlineData("/parts/00000000-0000-0000-0000-000000000001", "PUT")]
    [InlineData("/supplies/00000000-0000-0000-0000-000000000001", "PUT")]
    [InlineData("/customers/00000000-0000-0000-0000-000000000001", "DELETE")]
    [InlineData("/vehicles/00000000-0000-0000-0000-000000000001", "DELETE")]
    [InlineData("/suppliers/00000000-0000-0000-0000-000000000001", "DELETE")]
    [InlineData("/employees/00000000-0000-0000-0000-000000000001", "DELETE")]
    [InlineData("/services/00000000-0000-0000-0000-000000000001", "DELETE")]
    [InlineData("/parts/00000000-0000-0000-0000-000000000001", "DELETE")]
    [InlineData("/supplies/00000000-0000-0000-0000-000000000001", "DELETE")]
    public async Task WriteEndpoints_WithNonAdministrativeRole_Return403(string url, string method)
    {
        var client = CreateClientWithRole("FrontDesk");
        var request = CreateWriteRequest(url, method);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Theory]
    [InlineData("/customers", "POST")]
    [InlineData("/vehicles", "POST")]
    [InlineData("/suppliers", "POST")]
    [InlineData("/employees", "POST")]
    [InlineData("/services", "POST")]
    [InlineData("/parts", "POST")]
    [InlineData("/supplies", "POST")]
    [InlineData("/customers/00000000-0000-0000-0000-000000000001", "PUT")]
    [InlineData("/vehicles/00000000-0000-0000-0000-000000000001", "PUT")]
    [InlineData("/suppliers/00000000-0000-0000-0000-000000000001", "PUT")]
    [InlineData("/employees/00000000-0000-0000-0000-000000000001", "PUT")]
    [InlineData("/services/00000000-0000-0000-0000-000000000001", "PUT")]
    [InlineData("/parts/00000000-0000-0000-0000-000000000001", "PUT")]
    [InlineData("/supplies/00000000-0000-0000-0000-000000000001", "PUT")]
    [InlineData("/customers/00000000-0000-0000-0000-000000000001", "DELETE")]
    [InlineData("/vehicles/00000000-0000-0000-0000-000000000001", "DELETE")]
    [InlineData("/suppliers/00000000-0000-0000-0000-000000000001", "DELETE")]
    [InlineData("/employees/00000000-0000-0000-0000-000000000001", "DELETE")]
    [InlineData("/services/00000000-0000-0000-0000-000000000001", "DELETE")]
    [InlineData("/parts/00000000-0000-0000-0000-000000000001", "DELETE")]
    [InlineData("/supplies/00000000-0000-0000-0000-000000000001", "DELETE")]
    public async Task WriteEndpoints_WithAdministrativeRole_Not401Or403(string url, string method)
    {
        var client = CreateClientWithRole("Administrative");
        var request = CreateWriteRequest(url, method);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    private static HttpRequestMessage CreateWriteRequest(string url, string method)
    {
        var request = new HttpRequestMessage(new HttpMethod(method), url);

        if (method is "POST" or "PUT")
            request.Content = JsonContent.Create(new { });

        return request;
    }
}
