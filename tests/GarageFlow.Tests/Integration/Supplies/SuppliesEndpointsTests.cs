using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using GarageFlow.Api.Supplies.DTOs;
using GarageFlow.Tests.Integration;

namespace GarageFlow.Tests.Integration.Supplies;

public sealed class SuppliesEndpointsTests(GarageFlowWebApplicationFactory factory)
    : IClassFixture<GarageFlowWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static CreateSupplyRequest ValidRequest(string code, string name) =>
        new(name, code, "L", 25.00m, null);

    private async Task<SupplyResponse?> CreateSupply(string code, string name)
    {
        var response = await _client.PostAsJsonAsync("/supplies", ValidRequest(code, name));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return await response.Content.ReadFromJsonAsync<SupplyResponse>(JsonOptions);
    }

    [Fact]
    public async Task PostSupply_WithValidData_Returns201()
    {
        var response = await _client.PostAsJsonAsync("/supplies", ValidRequest("INT-INS-001", "Óleo Motor Integração 1"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task PostSupply_WithInvalidUnitOfMeasure_Returns400()
    {
        var request = new CreateSupplyRequest("Óleo Motor", "INT-INS-002", "INVALID", 25.00m, null);

        var response = await _client.PostAsJsonAsync("/supplies", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostSupply_WithNegativeBaseCost_Returns400()
    {
        var request = new CreateSupplyRequest("Óleo Motor", "INT-INS-003", "L", -1.00m, null);

        var response = await _client.PostAsJsonAsync("/supplies", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostSupply_WithDuplicateCode_Returns409()
    {
        await CreateSupply("INT-INS-010", "Óleo Motor 10");

        var response = await _client.PostAsJsonAsync("/supplies", ValidRequest("INT-INS-010", "Óleo Motor 10 Duplicado"));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetSupplyById_WithExistingId_Returns200()
    {
        var created = await CreateSupply("INT-INS-020", "Filtro de Ar 20");

        var response = await _client.GetAsync($"/supplies/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var supply = await response.Content.ReadFromJsonAsync<SupplyResponse>(JsonOptions);
        supply!.Code.Should().Be("INT-INS-020");
    }

    [Fact]
    public async Task GetSupplyById_WithNonExistentId_Returns404()
    {
        var response = await _client.GetAsync($"/supplies/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetSupplies_WithInvalidPage_Returns400()
    {
        var response = await _client.GetAsync("/supplies?page=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetSupplies_WithInvalidPageSize_Returns400()
    {
        var response = await _client.GetAsync("/supplies?page=1&pageSize=0");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetSupplies_WithValidPagination_Returns200()
    {
        await CreateSupply("INT-INS-030", "Insumo Paginação 30");

        var response = await _client.GetAsync("/supplies?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paged = await response.Content.ReadFromJsonAsync<PagedSupplyResponse>(JsonOptions);
        paged!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task PutSupply_WithValidData_Returns200()
    {
        var supply = await CreateSupply("INT-INS-040", "Insumo Atualização 40");
        var updateRequest = new UpdateSupplyRequest("Insumo Atualizado 40", "KG", 50.00m, null);

        var response = await _client.PutAsJsonAsync($"/supplies/{supply!.Id}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<SupplyResponse>(JsonOptions);
        updated!.Name.Should().Be("Insumo Atualizado 40");
        updated.UnitOfMeasure.Should().Be("KG");
    }

    [Fact]
    public async Task PutSupply_WithNonExistentId_Returns404()
    {
        var updateRequest = new UpdateSupplyRequest("Nome", "L", 10.00m, null);

        var response = await _client.PutAsJsonAsync($"/supplies/{Guid.NewGuid()}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteSupply_ActiveSupply_Returns204()
    {
        var supply = await CreateSupply("INT-INS-050", "Insumo Delete 50");

        var response = await _client.DeleteAsync($"/supplies/{supply!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteSupply_AlreadyInactive_Returns400()
    {
        var supply = await CreateSupply("INT-INS-060", "Insumo Delete 60");

        await _client.DeleteAsync($"/supplies/{supply!.Id}");
        var secondDelete = await _client.DeleteAsync($"/supplies/{supply.Id}");

        secondDelete.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteSupply_WithNonExistentId_Returns404()
    {
        var response = await _client.DeleteAsync($"/supplies/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
