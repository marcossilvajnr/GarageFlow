using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using GarageFlow.Api.Suppliers.DTOs;
using GarageFlow.Tests.Integration;
using Microsoft.AspNetCore.Mvc;

namespace GarageFlow.Tests.Integration.Suppliers;

public sealed class SuppliersEndpointsTests(GarageFlowWebApplicationFactory factory)
    : IClassFixture<GarageFlowWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Valid CNPJ list for testing (mathematically valid CNPJs for isolated test runs).
    /// Each test should use a unique CNPJ from this list to avoid conflicts.
    /// </summary>
    private static readonly string[] ValidCnpjs = new[]
    {
        "11.222.333/0001-81",
        "11.222.333/0002-62",
        "11.222.333/0003-43",
        "11.222.333/0004-24",
        "11.222.333/0005-05",
        "11.222.333/0006-96",
        "11.222.333/0007-77",
        "11.222.333/0008-58",
        "11.222.333/0009-39",
        "11.222.333/0010-72",
    };

    private static CreateSupplierRequest ValidRequest(string cnpj) => new(
        "Fornecedor SA",
        cnpj,
        "contato@fornecedor.com",
        "11987654321",
        "Rua das Flores",
        "100",
        null,
        "Centro",
        "São Paulo",
        "SP",
        "01310100");

    private async Task<SupplierResponse?> CreateSupplier(string cnpj)
    {
        var response = await _client.PostAsJsonAsync("/suppliers", ValidRequest(cnpj));
        if (response.StatusCode != HttpStatusCode.Created)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"CREATE SUPPLIER FAILED: CNPJ={cnpj}, Status={response.StatusCode}, Content={errorContent}");
        }
        return await response.Content.ReadFromJsonAsync<SupplierResponse>(JsonOptions);
    }

    [Fact]
    public async Task PostSupplier_WithValidData_Returns201()
    {
        var cnpj = ValidCnpjs[0];
        var request = ValidRequest(cnpj);

        var response = await _client.PostAsJsonAsync("/suppliers", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<SupplierResponse>(JsonOptions);
        body.Should().NotBeNull();
        body!.Id.Should().NotBeEmpty();
        body.IsActive.Should().BeTrue();
        body.Name.Should().Be("Fornecedor SA");
        body.Cnpj.Should().Be("11222333000181");
    }

    [Fact]
    public async Task PostSupplier_WithInvalidEmail_Returns400()
    {
        var cnpj = ValidCnpjs[1];
        var request = new CreateSupplierRequest(
            "Fornecedor SA",
            cnpj,
            "invalid-email",
            "11987654321",
            "Rua das Flores",
            "100",
            null,
            "Centro",
            "São Paulo",
            "SP",
            "01310100");

        var response = await _client.PostAsJsonAsync("/suppliers", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostSupplier_WithDuplicateCnpj_Returns409()
    {
        var cnpj = ValidCnpjs[2];
        var supplier = await CreateSupplier(cnpj);
        supplier.Should().NotBeNull();

        var response = await _client.PostAsJsonAsync("/suppliers", ValidRequest(cnpj));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetSupplierById_WithExistingId_Returns200()
    {
        var supplier = await CreateSupplier(ValidCnpjs[3]);
        supplier.Should().NotBeNull();

        var response = await _client.GetAsync($"/suppliers/{supplier!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<SupplierResponse>(JsonOptions);
        body.Should().NotBeNull();
        body!.Id.Should().Be(supplier.Id);
        body.Name.Should().Be("Fornecedor SA");
    }

    [Fact]
    public async Task GetSupplierById_WithNonExistentId_Returns404()
    {
        var response = await _client.GetAsync($"/suppliers/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>(JsonOptions);
        body!.Status.Should().Be(404);
        body.Title.Should().Be("Não encontrado");
    }

    [Fact]
    public async Task GetSuppliers_WithValidPagination_Returns200()
    {
        var supplier = await CreateSupplier(ValidCnpjs[4]);
        supplier.Should().NotBeNull();

        var response = await _client.GetAsync("/suppliers?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedSupplierResponse>(JsonOptions);
        body.Should().NotBeNull();
        body!.Items.Should().NotBeEmpty();
        body.Page.Should().Be(1);
    }

    [Fact]
    public async Task GetSuppliers_WithInvalidPage_Returns400()
    {
        var response = await _client.GetAsync("/suppliers?page=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetSuppliers_WithInvalidPageSize_Returns400()
    {
        var response = await _client.GetAsync("/suppliers?page=1&pageSize=0");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutSupplier_WithValidData_Returns200()
    {
        var supplier = await CreateSupplier(ValidCnpjs[5]);
        supplier.Should().NotBeNull();

        var updateRequest = new UpdateSupplierRequest(
            "Fornecedor Ltda",
            "novo@fornecedor.com",
            "11987654321",
            "Av. Paulista",
            "1000",
            null,
            "Bela Vista",
            "São Paulo",
            "SP",
            "01310100");

        var response = await _client.PutAsJsonAsync($"/suppliers/{supplier!.Id}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<SupplierResponse>(JsonOptions);
        body.Should().NotBeNull();
        body!.Name.Should().Be("Fornecedor Ltda");
        body.Email.Should().Be("novo@fornecedor.com");
    }

    [Fact]
    public async Task PutSupplier_WithInvalidEmail_Returns400()
    {
        var supplier = await CreateSupplier(ValidCnpjs[6]);
        supplier.Should().NotBeNull();

        var updateRequest = new UpdateSupplierRequest(
            "Fornecedor Ltda",
            "invalid-email",
            "11987654321",
            "Av. Paulista",
            "1000",
            null,
            "Bela Vista",
            "São Paulo",
            "SP",
            "01310100");

        var response = await _client.PutAsJsonAsync($"/suppliers/{supplier!.Id}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutSupplier_WithNonExistentId_Returns404()
    {
        var updateRequest = new UpdateSupplierRequest(
            "Fornecedor Ltda",
            "novo@fornecedor.com",
            "11987654321",
            "Av. Paulista",
            "1000",
            null,
            "Bela Vista",
            "São Paulo",
            "SP",
            "01310100");

        var response = await _client.PutAsJsonAsync($"/suppliers/{Guid.NewGuid()}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteSupplier_WithValidId_Returns204()
    {
        var supplier = await CreateSupplier(ValidCnpjs[7]);
        supplier.Should().NotBeNull();

        var response = await _client.DeleteAsync($"/suppliers/{supplier!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteSupplier_WithNonExistentId_Returns404()
    {
        var response = await _client.DeleteAsync($"/suppliers/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteSupplier_AlreadyInactive_Returns400()
    {
        var supplier = await CreateSupplier(ValidCnpjs[8]);
        supplier.Should().NotBeNull();

        await _client.DeleteAsync($"/suppliers/{supplier!.Id}");
        var secondDelete = await _client.DeleteAsync($"/suppliers/{supplier.Id}");

        secondDelete.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
