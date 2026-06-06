using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using GarageFlow.Api.Parts.DTOs;
using GarageFlow.Tests.Integration;
using Microsoft.AspNetCore.Mvc;

namespace GarageFlow.Tests.Integration.Parts;

public sealed class PartsEndpointsTests(GarageFlowWebApplicationFactory factory)
    : IClassFixture<GarageFlowWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static CreatePartRequest ValidRequest(string code, string sku, string name) =>
        new(name, code, sku, "UN", 25.00m);

    private async Task<PartResponse?> CreatePart(string code, string sku, string name)
    {
        var response = await _client.PostAsJsonAsync("/parts", ValidRequest(code, sku, name));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return await response.Content.ReadFromJsonAsync<PartResponse>(JsonOptions);
    }

    [Fact]
    public async Task PostPart_WithValidData_Returns201()
    {
        var response = await _client.PostAsJsonAsync("/parts", ValidRequest("INT-PRT-001", "INT-SKU-001", "Filtro Integração 1"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task PostPart_WithDuplicateSku_Returns409()
    {
        await CreatePart("INT-PRT-010", "INT-SKU-010", "Filtro 10");

        var response = await _client.PostAsJsonAsync("/parts", ValidRequest("INT-PRT-011", "INT-SKU-010", "Filtro 11"));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetParts_WithInvalidPage_Returns400()
    {
        var response = await _client.GetAsync("/parts?page=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetPartById_WithNonExistentId_Returns404()
    {
        var response = await _client.GetAsync($"/parts/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>(JsonOptions);
        body!.Status.Should().Be(404);
        body.Title.Should().Be("Não encontrado");
    }

    [Fact]
    public async Task PutPart_WithValidData_Returns200()
    {
        var part = await CreatePart("INT-PRT-020", "INT-SKU-020", "Filtro Int 20");
        var updateRequest = new UpdatePartRequest("Filtro Int 20 Atualizado", "KG", 50.00m);

        var response = await _client.PutAsJsonAsync($"/parts/{part!.Id}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeletePart_AlreadyInactive_Returns400()
    {
        var part = await CreatePart("INT-PRT-030", "INT-SKU-030", "Filtro Int 30");

        await _client.DeleteAsync($"/parts/{part!.Id}");
        var secondDelete = await _client.DeleteAsync($"/parts/{part.Id}");

        secondDelete.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
