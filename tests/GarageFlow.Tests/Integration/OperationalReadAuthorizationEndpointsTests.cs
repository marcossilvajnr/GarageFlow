using System.Net;
using FluentAssertions;

namespace GarageFlow.Tests.Integration;

public sealed class OperationalReadAuthorizationEndpointsTests(GarageFlowWebApplicationFactory factory)
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
    [InlineData("/service-orders?page=1&pageSize=5")]
    [InlineData("/service-orders/00000000-0000-0000-0000-000000000001")]
    [InlineData("/service-orders/00000000-0000-0000-0000-000000000001/quote")]
    [InlineData("/execution-orders?page=1&pageSize=5")]
    [InlineData("/execution-orders/00000000-0000-0000-0000-000000000001")]
    [InlineData("/separation-orders?page=1&pageSize=5")]
    [InlineData("/separation-orders/00000000-0000-0000-0000-000000000001")]
    [InlineData("/purchase-orders?page=1&pageSize=5")]
    [InlineData("/purchase-orders/00000000-0000-0000-0000-000000000001")]
    public async Task OperationalReadEndpoints_WithoutToken_Return401(string url)
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync(url);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("/service-orders?page=1&pageSize=5", "Stockist")]
    [InlineData("/service-orders/00000000-0000-0000-0000-000000000001", "Stockist")]
    [InlineData("/service-orders/00000000-0000-0000-0000-000000000001/quote", "Mechanic")]
    [InlineData("/execution-orders?page=1&pageSize=5", "FrontDesk")]
    [InlineData("/execution-orders/00000000-0000-0000-0000-000000000001", "FrontDesk")]
    [InlineData("/separation-orders?page=1&pageSize=5", "FrontDesk")]
    [InlineData("/separation-orders/00000000-0000-0000-0000-000000000001", "FrontDesk")]
    [InlineData("/purchase-orders?page=1&pageSize=5", "Mechanic")]
    [InlineData("/purchase-orders/00000000-0000-0000-0000-000000000001", "Mechanic")]
    public async Task OperationalReadEndpoints_WithForbiddenRole_Return403(string url, string role)
    {
        var client = CreateClientWithRole(role);

        var response = await client.GetAsync(url);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Theory]
    [InlineData("/service-orders?page=1&pageSize=5", "Mechanic", HttpStatusCode.OK)]
    [InlineData("/service-orders/00000000-0000-0000-0000-000000000001", "Mechanic", HttpStatusCode.NotFound)]
    [InlineData("/service-orders/00000000-0000-0000-0000-000000000001/quote", "FrontDesk", HttpStatusCode.NotFound)]
    [InlineData("/execution-orders?page=1&pageSize=5", "Mechanic", HttpStatusCode.OK)]
    [InlineData("/execution-orders/00000000-0000-0000-0000-000000000001", "Mechanic", HttpStatusCode.NotFound)]
    [InlineData("/separation-orders?page=1&pageSize=5", "Mechanic", HttpStatusCode.OK)]
    [InlineData("/separation-orders/00000000-0000-0000-0000-000000000001", "Mechanic", HttpStatusCode.NotFound)]
    [InlineData("/purchase-orders?page=1&pageSize=5", "Stockist", HttpStatusCode.OK)]
    [InlineData("/purchase-orders/00000000-0000-0000-0000-000000000001", "Stockist", HttpStatusCode.NotFound)]
    public async Task OperationalReadEndpoints_WithAllowedRole_ReturnExpectedStatus(
        string url,
        string role,
        HttpStatusCode expectedStatusCode)
    {
        var client = CreateClientWithRole(role);

        var response = await client.GetAsync(url);

        response.StatusCode.Should().Be(expectedStatusCode);
    }
}
