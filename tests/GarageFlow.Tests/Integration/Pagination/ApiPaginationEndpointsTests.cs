using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace GarageFlow.Tests.Integration.Pagination;

public sealed class ApiPaginationEndpointsTests(GarageFlowWebApplicationFactory factory)
    : IClassFixture<GarageFlowWebApplicationFactory>
{
    private const string InvalidPaginationDetail =
        "'page' deve ser maior ou igual a 1 e 'pageSize' deve estar entre 1 e 100.";

    private readonly HttpClient _client = factory.CreateClient();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Theory]
    [InlineData("/customers?page=1&pageSize=101")]
    [InlineData("/employees?page=1&pageSize=101")]
    [InlineData("/vehicles?page=1&pageSize=101")]
    [InlineData("/suppliers?page=1&pageSize=101")]
    [InlineData("/parts?page=1&pageSize=101")]
    [InlineData("/supplies?page=1&pageSize=101")]
    [InlineData("/services?page=1&pageSize=101")]
    [InlineData("/service-orders?page=1&pageSize=101")]
    [InlineData("/separation-orders?page=1&pageSize=101")]
    [InlineData("/execution-orders?page=1&pageSize=101")]
    [InlineData("/purchase-orders?page=1&pageSize=101")]
    public async Task ListEndpoint_WithPageSizeAboveMax_Returns400(string url)
    {
        var response = await _client.GetAsync(url);

        await AssertInvalidPaginationProblemDetails(response);
    }

    [Fact]
    public async Task ListStockOperations_WithPageSizeAboveMax_Returns400()
    {
        var response = await _client.GetAsync($"/stock/Part/{Guid.NewGuid()}/operations?page=1&pageSize=101");

        await AssertInvalidPaginationProblemDetails(response);
    }

    [Fact]
    public async Task ListEndpoint_WithPageSizeAtMax_Returns200()
    {
        var response = await _client.GetAsync("/customers?page=1&pageSize=100");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static async Task AssertInvalidPaginationProblemDetails(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>(JsonOptions);
        body.Should().NotBeNull();
        body!.Status.Should().Be(400);
        body.Title.Should().Be("Erro de validação");
        body.Detail.Should().Be(InvalidPaginationDetail);
    }
}
