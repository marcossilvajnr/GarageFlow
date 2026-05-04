using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using GarageFlow.Api.DTOs.Customers;
using GarageFlow.Api.DTOs.ServiceOrders;
using GarageFlow.Api.DTOs.Vehicles;
using GarageFlow.Domain.Customers;
using GarageFlow.Domain.ServiceOrders;
using GarageFlow.Tests.Integration;

namespace GarageFlow.Tests.Integration.ServiceOrders;

public sealed class ServiceOrdersEndpointsTests(GarageFlowWebApplicationFactory factory)
    : IClassFixture<GarageFlowWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private async Task<CustomerResponse> CreateCustomer(string document)
    {
        var request = new CreateCustomerRequest(
            "João Silva", CustomerDocumentType.Cpf, document,
            "joao@email.com", "11987654321",
            "Rua das Flores", "100", null, "Centro", "São Paulo", "SP", "01310100");

        var response = await _client.PostAsJsonAsync("/customers", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CustomerResponse>(JsonOptions))!;
    }

    private async Task<VehicleResponse> CreateVehicle(Guid customerId, string licensePlate, string renavam)
    {
        var request = new CreateVehicleRequest(customerId, licensePlate, renavam, "Toyota", "Corolla", 2020, "Branco");
        var response = await _client.PostAsJsonAsync("/vehicles", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<VehicleResponse>(JsonOptions))!;
    }

    private async Task<ServiceOrderResponse> CreateServiceOrder(Guid customerId, Guid vehicleId)
    {
        var request = new CreateServiceOrderRequest(customerId, vehicleId);
        var response = await _client.PostAsJsonAsync("/service-orders", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ServiceOrderResponse>(JsonOptions))!;
    }

    [Fact]
    public async Task PostServiceOrder_WithValidData_Returns201WithStatusReceived()
    {
        var customer = await CreateCustomer("529.982.247-25");
        var vehicle = await CreateVehicle(customer.Id, "ABC-1234", "11144477731");

        var request = new CreateServiceOrderRequest(customer.Id, vehicle.Id);
        var response = await _client.PostAsJsonAsync("/service-orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ServiceOrderResponse>(JsonOptions);
        body!.Id.Should().NotBeEmpty();
        body.CustomerId.Should().Be(customer.Id);
        body.VehicleId.Should().Be(vehicle.Id);
        body.Status.Should().Be(ServiceOrderStatus.Received);
    }

    [Fact]
    public async Task PostServiceOrder_WithNonExistentCustomer_Returns404()
    {
        var request = new CreateServiceOrderRequest(Guid.NewGuid(), Guid.NewGuid());
        var response = await _client.PostAsJsonAsync("/service-orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostServiceOrder_WithNonExistentVehicle_Returns404()
    {
        var customer = await CreateCustomer("123.456.789-09");
        var request = new CreateServiceOrderRequest(customer.Id, Guid.NewGuid());
        var response = await _client.PostAsJsonAsync("/service-orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostServiceOrder_WithEmptyCustomerId_Returns400()
    {
        var request = new CreateServiceOrderRequest(Guid.Empty, Guid.NewGuid());
        var response = await _client.PostAsJsonAsync("/service-orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostServiceOrder_WithEmptyVehicleId_Returns400()
    {
        var customer = await CreateCustomer("444.555.666-19");
        var request = new CreateServiceOrderRequest(customer.Id, Guid.Empty);
        var response = await _client.PostAsJsonAsync("/service-orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetServiceOrderById_Existing_Returns200()
    {
        var customer = await CreateCustomer("987.654.321-00");
        var vehicle = await CreateVehicle(customer.Id, "DEF-5678", "10000000090");
        var created = await CreateServiceOrder(customer.Id, vehicle.Id);

        var response = await _client.GetAsync($"/service-orders/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ServiceOrderResponse>(JsonOptions);
        body!.Id.Should().Be(created.Id);
        body.Status.Should().Be(ServiceOrderStatus.Received);
    }

    [Fact]
    public async Task GetServiceOrderById_NotFound_Returns404()
    {
        var response = await _client.GetAsync($"/service-orders/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetServiceOrders_Returns200WithPagination()
    {
        var customer = await CreateCustomer("111.444.777-35");
        var vehicle = await CreateVehicle(customer.Id, "GHI-9012", "12345678900");
        await CreateServiceOrder(customer.Id, vehicle.Id);

        var response = await _client.GetAsync("/service-orders?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedServiceOrderResponse>(JsonOptions);
        body!.Items.Should().NotBeEmpty();
        body.Page.Should().Be(1);
        body.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetServiceOrders_InvalidPage_Returns400()
    {
        var response = await _client.GetAsync("/service-orders?page=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetServiceOrders_InvalidPageSize_Returns400()
    {
        var response = await _client.GetAsync("/service-orders?page=1&pageSize=0");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
