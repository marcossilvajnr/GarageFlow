using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GarageFlow.Api.DTOs.Customers;
using GarageFlow.Tests.E2E.Builders;
using GarageFlow.Tests.E2E.Infrastructure;
using GarageFlow.Tests.Integration;

namespace GarageFlow.Tests.E2E.Smoke;

public sealed class E2ESmokeTests : E2ETestBase, IClassFixture<GarageFlowWebApplicationFactory>
{
    private readonly HttpClient _client;

    public E2ESmokeTests(GarageFlowWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Smoke_CreateAndGetCustomer_ShouldWorkEndToEnd()
    {
        var seed = new E2ESeedBuilder(_client);
        var customer = await seed.CreateCustomerAsync("529.982.247-25");

        var getResponse = await _client.GetAsync($"/customers/{customer.Id}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await getResponse.Content.ReadFromJsonAsync<CustomerResponse>(JsonOptions);
        body.Should().NotBeNull();
        body!.Id.Should().Be(customer.Id);
        body.Name.Should().Be("João E2E");
    }
}
