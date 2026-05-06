using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GarageFlow.Api.DTOs.Customers;
using GarageFlow.Api.DTOs.Executions;
using GarageFlow.Api.DTOs.Parts;
using GarageFlow.Api.DTOs.ServiceOrders;
using GarageFlow.Api.DTOs.Services;
using GarageFlow.Api.DTOs.Stock;
using GarageFlow.Api.DTOs.Supplies;
using GarageFlow.Api.DTOs.Vehicles;
using GarageFlow.Domain.Customers;
using GarageFlow.Domain.Executions;
using GarageFlow.Domain.ServiceOrders;
using GarageFlow.Domain.Stock;
using GarageFlow.Domain.Supplies;
using GarageFlow.Tests.E2E.Infrastructure;

namespace GarageFlow.Tests.E2E.ServiceOrders;

[Collection("E2E Real DB")]
public sealed class ServiceOrderSufficientStockE2ETests : E2ETestBase
{
    private readonly HttpClient _client;

    private static int _cpfSeed = 200_000_000;
    private static int _renavamSeed = 2_000_000_000;
    private static int _licensePlateSeed;

    public ServiceOrderSufficientStockE2ETests(E2ERealDbWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HappyPath_ServiceOrderWithSufficientStock_ShouldReachCanonicalFinalStates()
    {
        await ResetRealDatabaseAsync(_client);
        ClearAuthentication(_client);

        var unauthorizedResponse = await _client.GetAsync("/employees?page=1&pageSize=10");
        unauthorizedResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        await AuthenticateAsAsync(_client, E2ERole.Mechanic);
        var forbiddenResponse = await _client.PostAsJsonAsync(
            "/stock/releases",
            new ReleaseStockReservationRequest(
                Guid.NewGuid(),
                StockItemType.Part,
                1m,
                "Tentativa sem permissão",
                "mechanic.e2e",
                null,
                null));
        forbiddenResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        await AuthenticateAsAsync(_client, E2ERole.Administrative);

        var customer = await CreateCustomerAsync();
        var vehicle = await CreateVehicleAsync(customer.Id);
        var part = await CreatePartAsync();
        var supply = await CreateSupplyAsync();
        var service = await CreateServiceAsync();

        var addPartResponse = await _client.PostAsJsonAsync(
            $"/services/{service.Id}/parts",
            new AddServicePartRequest(part.Id, 2));
        addPartResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var addSupplyResponse = await _client.PostAsJsonAsync(
            $"/services/{service.Id}/supplies",
            new AddServiceSupplyRequest(supply.Id, 0.5m));
        addSupplyResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var createServiceOrderResponse = await _client.PostAsJsonAsync(
            "/service-orders",
            new CreateServiceOrderRequest(customer.Id, vehicle.Id));
        createServiceOrderResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var serviceOrder = await ReadAsync<ServiceOrderResponse>(createServiceOrderResponse);
        serviceOrder.Status.Should().Be(ServiceOrderStatus.Received);

        var startDiagnosticResponse = await _client.PostAsJsonAsync(
            $"/service-orders/{serviceOrder.Id}/diagnostic/start",
            new StartDiagnosticRequest(Guid.NewGuid()));
        startDiagnosticResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var serviceOrderInDiagnostic = await ReadAsync<ServiceOrderResponse>(startDiagnosticResponse);
        serviceOrderInDiagnostic.Status.Should().Be(ServiceOrderStatus.InDiagnostic);

        var addDiagnosticServiceResponse = await _client.PostAsJsonAsync(
            $"/service-orders/{serviceOrder.Id}/diagnostic/services",
            new AddDiagnosticServiceRequest(service.Id));
        addDiagnosticServiceResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var completeDiagnosticResponse = await _client.PostAsJsonAsync(
            $"/service-orders/{serviceOrder.Id}/diagnostic/complete",
            new CompleteDiagnosticRequest("Diagnóstico E2E: serviço confirmado."));
        completeDiagnosticResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var consolidateResponse = await _client.PostAsync(
            $"/service-orders/{serviceOrder.Id}/diagnostic/consolidate-services",
            null);
        consolidateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var generateQuoteResponse = await _client.PostAsync(
            $"/service-orders/{serviceOrder.Id}/quote/generate",
            null);
        generateQuoteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var generatedQuote = await ReadAsync<QuoteResponse>(generateQuoteResponse);
        generatedQuote.Status.Should().Be(QuoteStatus.WaitingCustomerApproval);

        var acceptQuoteResponse = await _client.PostAsync(
            $"/service-orders/{serviceOrder.Id}/quote/accept",
            null);
        acceptQuoteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var acceptedQuote = await ReadAsync<QuoteResponse>(acceptQuoteResponse);
        acceptedQuote.Status.Should().Be(QuoteStatus.CustomerApproved);

        var approvedServiceOrder = await GetServiceOrderAsync(serviceOrder.Id);
        approvedServiceOrder.Status.Should().Be(ServiceOrderStatus.Approved);

        var createExecutionResponse = await _client.PostAsJsonAsync(
            "/execution-orders",
            new CreateExecutionOrderRequest(serviceOrder.Id, service.Id));
        createExecutionResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var executionOrder = await ReadAsync<ExecutionOrderResponse>(createExecutionResponse);
        executionOrder.Status.Should().Be(ExecutionOrderStatus.Pending);

        await SeedStockAsync(part.Id, StockItemType.Part, 20m);
        await SeedStockAsync(supply.Id, StockItemType.Supply, 20m);

        var createSeparationResponse = await _client.PostAsJsonAsync(
            "/separation-orders",
            new CreateSeparationOrderRequest(
                executionOrder.Id,
                [new CreateSeparationPartItemRequest(part.Id, part.Name, 2)],
                [new CreateSeparationSupplyItemRequest(supply.Id, supply.Name, 0.5m, SupplyUnit.Liter)]));
        createSeparationResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var separationOrder = await ReadAsync<SeparationOrderResponse>(createSeparationResponse);
        separationOrder.Status.Should().Be(SeparationOrderStatus.Pending);

        var reserveResponse = await _client.PostAsync($"/separation-orders/{separationOrder.Id}/reserve", null);
        reserveResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var waitingPickupSeparation = await ReadAsync<SeparationOrderResponse>(reserveResponse);
        waitingPickupSeparation.Status.Should().Be(SeparationOrderStatus.WaitingPickup);

        var startBeforeReadyResponse = await _client.PostAsJsonAsync(
            $"/execution-orders/{executionOrder.Id}/start",
            new StartExecutionOrderRequest(Guid.NewGuid()));
        startBeforeReadyResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var mechanicBeforeStockistResponse = await _client.PostAsync(
            $"/separation-orders/{separationOrder.Id}/confirm-mechanic-receipt",
            null);
        mechanicBeforeStockistResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var confirmStockistResponse = await _client.PostAsJsonAsync(
            $"/separation-orders/{separationOrder.Id}/confirm-stockist-withdrawal",
            new ConfirmSeparationStockistWithdrawalRequest(Guid.NewGuid()));
        confirmStockistResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var separatedOrder = await ReadAsync<SeparationOrderResponse>(confirmStockistResponse);
        separatedOrder.Status.Should().Be(SeparationOrderStatus.Separated);

        var confirmMechanicResponse = await _client.PostAsync(
            $"/separation-orders/{separationOrder.Id}/confirm-mechanic-receipt",
            null);
        confirmMechanicResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var completedSeparationOrder = await ReadAsync<SeparationOrderResponse>(confirmMechanicResponse);
        completedSeparationOrder.Status.Should().Be(SeparationOrderStatus.Completed);

        var readyExecutionOrderResponse = await _client.GetAsync($"/execution-orders/{executionOrder.Id}");
        readyExecutionOrderResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var readyExecutionOrder = await ReadAsync<ExecutionOrderResponse>(readyExecutionOrderResponse);
        readyExecutionOrder.Status.Should().Be(ExecutionOrderStatus.Ready);

        var startExecutionResponse = await _client.PostAsJsonAsync(
            $"/execution-orders/{executionOrder.Id}/start",
            new StartExecutionOrderRequest(Guid.NewGuid()));
        startExecutionResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var inExecutionOrder = await ReadAsync<ExecutionOrderResponse>(startExecutionResponse);
        inExecutionOrder.Status.Should().Be(ExecutionOrderStatus.InExecution);

        var serviceOrderInExecution = await GetServiceOrderAsync(serviceOrder.Id);
        serviceOrderInExecution.Status.Should().Be(ServiceOrderStatus.InExecution);

        var deliverBeforeFinishResponse = await _client.PostAsync(
            $"/service-orders/{serviceOrder.Id}/deliver",
            null);
        deliverBeforeFinishResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var completeExecutionResponse = await _client.PostAsync(
            $"/execution-orders/{executionOrder.Id}/complete",
            null);
        completeExecutionResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var completedExecutionOrder = await ReadAsync<ExecutionOrderResponse>(completeExecutionResponse);
        completedExecutionOrder.Status.Should().Be(ExecutionOrderStatus.Completed);

        var finishedServiceOrder = await GetServiceOrderAsync(serviceOrder.Id);
        finishedServiceOrder.Status.Should().Be(ServiceOrderStatus.Finished);

        var deliverResponse = await _client.PostAsync(
            $"/service-orders/{serviceOrder.Id}/deliver",
            null);
        deliverResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var deliveredServiceOrder = await ReadAsync<ServiceOrderResponse>(deliverResponse);
        deliveredServiceOrder.Status.Should().Be(ServiceOrderStatus.Delivered);

        var finalSeparationResponse = await _client.GetAsync($"/separation-orders/{separationOrder.Id}");
        finalSeparationResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var finalSeparationOrder = await ReadAsync<SeparationOrderResponse>(finalSeparationResponse);
        finalSeparationOrder.Status.Should().Be(SeparationOrderStatus.Completed);

        var finalExecutionResponse = await _client.GetAsync($"/execution-orders/{executionOrder.Id}");
        finalExecutionResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var finalExecutionOrder = await ReadAsync<ExecutionOrderResponse>(finalExecutionResponse);
        finalExecutionOrder.Status.Should().Be(ExecutionOrderStatus.Completed);
    }

    private async Task<CustomerResponse> CreateCustomerAsync()
    {
        var response = await _client.PostAsJsonAsync(
            "/customers",
            new CreateCustomerRequest(
                "Cliente E2E",
                CustomerDocumentType.Cpf,
                GenerateValidCpf(),
                $"cliente-{Guid.NewGuid():N}@garageflow.test",
                "11987654321",
                "Rua E2E",
                "100",
                null,
                "Centro",
                "São Paulo",
                "SP",
                "01310100"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return await ReadAsync<CustomerResponse>(response);
    }

    private async Task<VehicleResponse> CreateVehicleAsync(Guid customerId)
    {
        var response = await _client.PostAsJsonAsync(
            "/vehicles",
            new CreateVehicleRequest(
                customerId,
                GenerateValidLicensePlate(),
                GenerateValidRenavam(),
                "Toyota",
                "Corolla",
                2020,
                "Prata"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return await ReadAsync<VehicleResponse>(response);
    }

    private async Task<PartResponse> CreatePartAsync()
    {
        var response = await _client.PostAsJsonAsync(
            "/parts",
            new CreatePartRequest(
                "Filtro E2E",
                UniqueCode("PRT", 10),
                UniqueCode("SKU", 12),
                "UN",
                35m));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return await ReadAsync<PartResponse>(response);
    }

    private async Task<SupplyResponse> CreateSupplyAsync()
    {
        var response = await _client.PostAsJsonAsync(
            "/supplies",
            new CreateSupplyRequest(
                "Óleo E2E",
                UniqueCode("SUP", 12),
                "L",
                22m,
                null));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return await ReadAsync<SupplyResponse>(response);
    }

    private async Task<ServiceResponse> CreateServiceAsync()
    {
        var response = await _client.PostAsJsonAsync(
            "/services",
            new CreateServiceRequest(
                UniqueCode("SVC", 12),
                "Serviço E2E Completo",
                "Fluxo happy path com estoque suficiente",
                180m,
                90));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return await ReadAsync<ServiceResponse>(response);
    }

    private async Task SeedStockAsync(Guid itemId, StockItemType itemType, decimal quantity)
    {
        var response = await _client.PostAsJsonAsync(
            "/stock/entries",
            new CreateStockEntryRequest(itemId, itemType, quantity, 0m, "Seed E2E", null));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task<ServiceOrderResponse> GetServiceOrderAsync(Guid serviceOrderId)
    {
        var response = await _client.GetAsync($"/service-orders/{serviceOrderId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await ReadAsync<ServiceOrderResponse>(response);
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response) where T : class
        => (await response.Content.ReadFromJsonAsync<T>(JsonOptions))!;

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
}
