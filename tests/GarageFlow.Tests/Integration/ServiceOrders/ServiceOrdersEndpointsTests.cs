using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using GarageFlow.Api.DTOs.Customers;
using GarageFlow.Api.DTOs.ServiceOrders;
using GarageFlow.Api.DTOs.Services;
using GarageFlow.Api.DTOs.Vehicles;
using GarageFlow.Domain.Customers;
using GarageFlow.Domain.ServiceOrders;
using GarageFlow.Tests.Integration;

namespace GarageFlow.Tests.Integration.ServiceOrders;

public sealed class ServiceOrdersEndpointsTests(GarageFlowWebApplicationFactory factory)
    : IClassFixture<GarageFlowWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private static int _cpfSeed = 100_000_000;
    private static int _renavamSeed = 1_000_000_000;
    private static int _licensePlateSeed;

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

    private static string GenerateValidCpf()
    {
        var baseDigits = Interlocked.Increment(ref _cpfSeed) % 1_000_000_000;
        var baseNumber = baseDigits.ToString("D9");

        var firstDigit = CalculateCpfVerifier(baseNumber, 10);
        var secondDigit = CalculateCpfVerifier(baseNumber + firstDigit, 11);
        var rawCpf = $"{baseNumber}{firstDigit}{secondDigit}";

        return $"{rawCpf[..3]}.{rawCpf.Substring(3, 3)}.{rawCpf.Substring(6, 3)}-{rawCpf.Substring(9, 2)}";
    }

    private static int CalculateCpfVerifier(string digits, int weightStart)
    {
        var sum = 0;
        for (var i = 0; i < digits.Length; i++)
        {
            sum += (digits[i] - '0') * (weightStart - i);
        }

        var mod = sum % 11;
        return mod < 2 ? 0 : 11 - mod;
    }

    private async Task<VehicleResponse> CreateVehicle(Guid customerId, string licensePlate, string renavam)
    {
        var request = new CreateVehicleRequest(customerId, licensePlate, renavam, "Toyota", "Corolla", 2020, "Branco");
        var response = await _client.PostAsJsonAsync("/vehicles", request);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"CreateVehicle failed with {(int)response.StatusCode}: {error}. Plate={licensePlate}, Renavam={renavam}");
        }
        return (await response.Content.ReadFromJsonAsync<VehicleResponse>(JsonOptions))!;
    }

    private static string GenerateValidLicensePlate()
    {
        var seed = Interlocked.Increment(ref _licensePlateSeed);
        var lettersSeed = seed / 10_000;
        var numbers = seed % 10_000;

        var first = (char)('A' + (lettersSeed / (26 * 26)) % 26);
        var second = (char)('A' + (lettersSeed / 26) % 26);
        var third = (char)('A' + lettersSeed % 26);

        return $"{first}{second}{third}-{numbers:D4}";
    }

    private static string GenerateValidRenavam()
    {
        var baseDigits = Interlocked.Increment(ref _renavamSeed).ToString("D10");
        var checkDigit = CalculateRenavamCheckDigit(baseDigits);
        return $"{baseDigits}{checkDigit}";
    }

    private static int CalculateRenavamCheckDigit(string firstTenDigits)
    {
        var multipliers = new[] { 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        var sum = 0;

        for (var i = 0; i < firstTenDigits.Length; i++)
        {
            sum += (firstTenDigits[i] - '0') * multipliers[i];
        }

        var remainder = sum % 11;
        return remainder == 10 ? 0 : remainder;
    }

    private async Task<ServiceOrderResponse> CreateServiceOrder(Guid customerId, Guid vehicleId)
    {
        var request = new CreateServiceOrderRequest(customerId, vehicleId);
        var response = await _client.PostAsJsonAsync("/service-orders", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ServiceOrderResponse>(JsonOptions))!;
    }

    private async Task<ServiceResponse> CreateService(string code, string name)
    {
        var request = new CreateServiceRequest(code, name, null, 100m, 60);
        var response = await _client.PostAsJsonAsync("/services", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ServiceResponse>(JsonOptions))!;
    }

    [Fact]
    public async Task PostServiceOrder_WithValidData_Returns201WithStatusReceived()
    {
        var customer = await CreateCustomer(GenerateValidCpf());
        var vehicle = await CreateVehicle(customer.Id, GenerateValidLicensePlate(), GenerateValidRenavam());

        var request = new CreateServiceOrderRequest(customer.Id, vehicle.Id);
        var response = await _client.PostAsJsonAsync("/service-orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ServiceOrderResponse>(JsonOptions);
        body!.Id.Should().NotBeEmpty();
        body.CustomerId.Should().Be(customer.Id);
        body.VehicleId.Should().Be(vehicle.Id);
        body.Status.Should().Be(ServiceOrderStatus.Received);
        body.Services.Should().BeEmpty();
        body.ServiceHistory.Should().BeEmpty();
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
        var customer = await CreateCustomer(GenerateValidCpf());
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
        var customer = await CreateCustomer(GenerateValidCpf());
        var request = new CreateServiceOrderRequest(customer.Id, Guid.Empty);
        var response = await _client.PostAsJsonAsync("/service-orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostServiceOrder_WithVehicleFromAnotherCustomer_Returns400()
    {
        var customer1 = await CreateCustomer(GenerateValidCpf());
        var customer2 = await CreateCustomer(GenerateValidCpf());
        var vehicle = await CreateVehicle(customer1.Id, GenerateValidLicensePlate(), GenerateValidRenavam());

        var request = new CreateServiceOrderRequest(customer2.Id, vehicle.Id);
        var response = await _client.PostAsJsonAsync("/service-orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetServiceOrderById_Existing_Returns200WithServicesAndHistory()
    {
        var customer = await CreateCustomer(GenerateValidCpf());
        var vehicle = await CreateVehicle(customer.Id, GenerateValidLicensePlate(), GenerateValidRenavam());
        var created = await CreateServiceOrder(customer.Id, vehicle.Id);

        var response = await _client.GetAsync($"/service-orders/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ServiceOrderResponse>(JsonOptions);
        body!.Id.Should().Be(created.Id);
        body.Status.Should().Be(ServiceOrderStatus.Received);
        body.Services.Should().NotBeNull();
        body.ServiceHistory.Should().NotBeNull();
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
        var customer = await CreateCustomer(GenerateValidCpf());
        var vehicle = await CreateVehicle(customer.Id, GenerateValidLicensePlate(), GenerateValidRenavam());
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

    // Task-012: AddService integration tests

    [Fact]
    public async Task PostServiceOrderService_WithValidData_Returns200WithUpdatedServiceOrder()
    {
        var customer = await CreateCustomer(GenerateValidCpf());
        var vehicle = await CreateVehicle(customer.Id, GenerateValidLicensePlate(), GenerateValidRenavam());
        var serviceOrder = await CreateServiceOrder(customer.Id, vehicle.Id);
        var service = await CreateService("SVC-012-001", "Troca de Óleo 001");

        var request = new AddServiceToServiceOrderRequest(service.Id, Guid.NewGuid());
        var response = await _client.PostAsJsonAsync($"/service-orders/{serviceOrder.Id}/services", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ServiceOrderResponse>(JsonOptions);
        body!.Services.Should().HaveCount(1);
        body.Services.Single().ServiceId.Should().Be(service.Id);
        body.Services.Single().IsActive.Should().BeTrue();
        body.ServiceHistory.Should().HaveCount(1);
        body.ServiceHistory.Single().Action.Should().Be(ServiceOrderServiceAction.Added);
    }

    [Fact]
    public async Task PostServiceOrderService_WithNonExistentServiceOrder_Returns404()
    {
        var service = await CreateService("SVC-012-002", "Troca de Correia 002");
        var request = new AddServiceToServiceOrderRequest(service.Id, Guid.NewGuid());

        var response = await _client.PostAsJsonAsync($"/service-orders/{Guid.NewGuid()}/services", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostServiceOrderService_WithNonExistentService_Returns404()
    {
        var customer = await CreateCustomer(GenerateValidCpf());
        var vehicle = await CreateVehicle(customer.Id, GenerateValidLicensePlate(), GenerateValidRenavam());
        var serviceOrder = await CreateServiceOrder(customer.Id, vehicle.Id);

        var request = new AddServiceToServiceOrderRequest(Guid.NewGuid(), Guid.NewGuid());
        var response = await _client.PostAsJsonAsync($"/service-orders/{serviceOrder.Id}/services", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostServiceOrderService_WithEmptyActorId_Returns400()
    {
        var customer = await CreateCustomer(GenerateValidCpf());
        var vehicle = await CreateVehicle(customer.Id, GenerateValidLicensePlate(), GenerateValidRenavam());
        var serviceOrder = await CreateServiceOrder(customer.Id, vehicle.Id);
        var service = await CreateService("SVC-012-003", "Balanceamento 003");

        var request = new AddServiceToServiceOrderRequest(service.Id, Guid.Empty);
        var response = await _client.PostAsJsonAsync($"/service-orders/{serviceOrder.Id}/services", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostServiceOrderService_DuplicateService_Returns409()
    {
        var customer = await CreateCustomer(GenerateValidCpf());
        var vehicle = await CreateVehicle(customer.Id, GenerateValidLicensePlate(), GenerateValidRenavam());
        var serviceOrder = await CreateServiceOrder(customer.Id, vehicle.Id);
        var service = await CreateService("SVC-012-004", "Alinhamento 004");

        var request = new AddServiceToServiceOrderRequest(service.Id, Guid.NewGuid());
        await _client.PostAsJsonAsync($"/service-orders/{serviceOrder.Id}/services", request);

        var response = await _client.PostAsJsonAsync($"/service-orders/{serviceOrder.Id}/services", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // Task-012: RemoveService integration tests

    [Fact]
    public async Task DeleteServiceOrderService_WithValidData_Returns204()
    {
        var customer = await CreateCustomer(GenerateValidCpf());
        var vehicle = await CreateVehicle(customer.Id, GenerateValidLicensePlate(), GenerateValidRenavam());
        var serviceOrder = await CreateServiceOrder(customer.Id, vehicle.Id);
        var service = await CreateService("SVC-012-005", "Diagnóstico Elétrico 005");

        var addRequest = new AddServiceToServiceOrderRequest(service.Id, Guid.NewGuid());
        await _client.PostAsJsonAsync($"/service-orders/{serviceOrder.Id}/services", addRequest);

        var removeRequest = new RemoveServiceFromServiceOrderRequest(Guid.NewGuid(), "Cliente desistiu");
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Delete,
            $"/service-orders/{serviceOrder.Id}/services/{service.Id}")
        {
            Content = JsonContent.Create(removeRequest)
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteServiceOrderService_WithNonExistentServiceOrder_Returns404()
    {
        var removeRequest = new RemoveServiceFromServiceOrderRequest(Guid.NewGuid(), "motivo");
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Delete,
            $"/service-orders/{Guid.NewGuid()}/services/{Guid.NewGuid()}")
        {
            Content = JsonContent.Create(removeRequest)
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteServiceOrderService_ServiceNotLinked_Returns404()
    {
        var customer = await CreateCustomer(GenerateValidCpf());
        var vehicle = await CreateVehicle(customer.Id, GenerateValidLicensePlate(), GenerateValidRenavam());
        var serviceOrder = await CreateServiceOrder(customer.Id, vehicle.Id);

        var removeRequest = new RemoveServiceFromServiceOrderRequest(Guid.NewGuid(), "motivo");
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Delete,
            $"/service-orders/{serviceOrder.Id}/services/{Guid.NewGuid()}")
        {
            Content = JsonContent.Create(removeRequest)
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteServiceOrderService_WithEmptyActorId_Returns400()
    {
        var customer = await CreateCustomer(GenerateValidCpf());
        var vehicle = await CreateVehicle(customer.Id, GenerateValidLicensePlate(), GenerateValidRenavam());
        var serviceOrder = await CreateServiceOrder(customer.Id, vehicle.Id);
        var service = await CreateService("SVC-012-006", "Revisão Completa 006");

        var addRequest = new AddServiceToServiceOrderRequest(service.Id, Guid.NewGuid());
        await _client.PostAsJsonAsync($"/service-orders/{serviceOrder.Id}/services", addRequest);

        var removeRequest = new RemoveServiceFromServiceOrderRequest(Guid.Empty, "motivo");
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Delete,
            $"/service-orders/{serviceOrder.Id}/services/{service.Id}")
        {
            Content = JsonContent.Create(removeRequest)
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteServiceOrderService_WithEmptyReason_Returns400()
    {
        var customer = await CreateCustomer(GenerateValidCpf());
        var vehicle = await CreateVehicle(customer.Id, GenerateValidLicensePlate(), GenerateValidRenavam());
        var serviceOrder = await CreateServiceOrder(customer.Id, vehicle.Id);
        var service = await CreateService("SVC-012-007", "Troca de Pastilhas 007");

        var addRequest = new AddServiceToServiceOrderRequest(service.Id, Guid.NewGuid());
        await _client.PostAsJsonAsync($"/service-orders/{serviceOrder.Id}/services", addRequest);

        var removeRequest = new RemoveServiceFromServiceOrderRequest(Guid.NewGuid(), "");
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Delete,
            $"/service-orders/{serviceOrder.Id}/services/{service.Id}")
        {
            Content = JsonContent.Create(removeRequest)
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetServiceOrderById_AfterAddAndRemove_ReturnsServicesAndHistoryWithTwoEntries()
    {
        var customer = await CreateCustomer(GenerateValidCpf());
        var vehicle = await CreateVehicle(customer.Id, GenerateValidLicensePlate(), GenerateValidRenavam());
        var serviceOrder = await CreateServiceOrder(customer.Id, vehicle.Id);
        var service = await CreateService("SVC-012-008", "Troca de Filtro 008");

        var addRequest = new AddServiceToServiceOrderRequest(service.Id, Guid.NewGuid());
        await _client.PostAsJsonAsync($"/service-orders/{serviceOrder.Id}/services", addRequest);

        var removeRequest = new RemoveServiceFromServiceOrderRequest(Guid.NewGuid(), "Serviço desnecessário");
        await _client.SendAsync(new HttpRequestMessage(HttpMethod.Delete,
            $"/service-orders/{serviceOrder.Id}/services/{service.Id}")
        {
            Content = JsonContent.Create(removeRequest)
        });

        var response = await _client.GetAsync($"/service-orders/{serviceOrder.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ServiceOrderResponse>(JsonOptions);
        body!.Services.Should().HaveCount(1);
        body.Services.Single().IsActive.Should().BeFalse();
        body.Services.Single().RemovalReason.Should().Be("Serviço desnecessário");
        body.ServiceHistory.Should().HaveCount(2);
    }
}
