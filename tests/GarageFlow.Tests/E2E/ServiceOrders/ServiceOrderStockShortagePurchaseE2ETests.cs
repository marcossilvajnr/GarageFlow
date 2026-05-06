using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GarageFlow.Api.DTOs.Customers;
using GarageFlow.Api.DTOs.Executions;
using GarageFlow.Api.DTOs.Parts;
using GarageFlow.Api.DTOs.Purchasing;
using GarageFlow.Api.DTOs.ServiceOrders;
using GarageFlow.Api.DTOs.Services;
using GarageFlow.Api.DTOs.Stock;
using GarageFlow.Api.DTOs.Suppliers;
using GarageFlow.Api.DTOs.Vehicles;
using GarageFlow.Domain.Customers;
using GarageFlow.Domain.Executions;
using GarageFlow.Domain.Purchasing;
using GarageFlow.Domain.ServiceOrders;
using GarageFlow.Domain.Stock;
using GarageFlow.Tests.E2E.Infrastructure;

namespace GarageFlow.Tests.E2E.ServiceOrders;

[Collection("E2E Real DB")]
public sealed class ServiceOrderStockShortagePurchaseE2ETests : E2ETestBase
{
    private readonly HttpClient _client;

    private static int _cpfSeed = 300_000_000;
    private static int _renavamSeed = 1_500_000_000;
    private static int _licensePlateSeed;
    private static int _supplierSeed;

    private static readonly string[] SupplierCnpjs =
    [
        "11.222.333/0001-81",
        "11.222.333/0002-62",
        "11.222.333/0003-43",
        "11.222.333/0004-24",
        "11.222.333/0005-05",
        "11.222.333/0006-96",
        "11.222.333/0007-77",
        "11.222.333/0008-58",
        "11.222.333/0009-39",
        "11.222.333/0010-72"
    ];

    public ServiceOrderStockShortagePurchaseE2ETests(E2ERealDbWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HappyPath_ServiceOrderWithStockShortageAndPurchase_ShouldReachCanonicalFinalStates()
    {
        await ResetRealDatabaseAsync(_client);
        await AuthenticateAsAsync(_client, E2ERole.Administrative);

        var customer = await CreateCustomerAsync();
        var vehicle = await CreateVehicleAsync(customer.Id);
        var part = await CreatePartAsync();
        var service = await CreateServiceAsync();
        var supplier = await CreateSupplierAsync();

        var addPartResponse = await _client.PostAsJsonAsync(
            $"/services/{service.Id}/parts",
            new AddServicePartRequest(part.Id, 2));
        addPartResponse.StatusCode.Should().Be(HttpStatusCode.OK);

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
            new CompleteDiagnosticRequest("Diagnóstico E2E com ruptura: compra necessária."));
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

        await SeedStockAsync(part.Id, StockItemType.Part, 1m);

        var createSeparationResponse = await _client.PostAsJsonAsync(
            "/separation-orders",
            new CreateSeparationOrderRequest(
                executionOrder.Id,
                [new CreateSeparationPartItemRequest(part.Id, part.Name, 2)],
                []));
        createSeparationResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var separationOrder = await ReadAsync<SeparationOrderResponse>(createSeparationResponse);
        separationOrder.Status.Should().Be(SeparationOrderStatus.Pending);

        var reserveWithShortageResponse = await _client.PostAsync($"/separation-orders/{separationOrder.Id}/reserve", null);
        reserveWithShortageResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var waitPurchaseResponse = await _client.PostAsync($"/separation-orders/{separationOrder.Id}/wait-purchase", null);
        waitPurchaseResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var waitingPurchaseOrder = await ReadAsync<SeparationOrderResponse>(waitPurchaseResponse);
        waitingPurchaseOrder.Status.Should().Be(SeparationOrderStatus.WaitingPurchase);

        var startBeforeReadyResponse = await _client.PostAsJsonAsync(
            $"/execution-orders/{executionOrder.Id}/start",
            new StartExecutionOrderRequest(Guid.NewGuid()));
        startBeforeReadyResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var createPurchaseResponse = await _client.PostAsJsonAsync(
            "/purchase-orders",
            new CreatePurchaseOrderRequest(
                [separationOrder.Id],
                [new CreatePurchaseItemRequest(part.Id, PurchaseItemType.Part, part.Name, 2m, part.UnitPrice)]));
        createPurchaseResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var purchaseOrder = await ReadAsync<PurchaseOrderResponse>(createPurchaseResponse);
        purchaseOrder.Status.Should().Be(PurchaseOrderStatus.Created);

        var assignSupplierResponse = await _client.PostAsJsonAsync(
            $"/purchase-orders/{purchaseOrder.Id}/assign-supplier",
            new AssignPurchaseOrderSupplierRequest(supplier.Id));
        assignSupplierResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var purchaseWithSupplier = await ReadAsync<PurchaseOrderResponse>(assignSupplierResponse);
        purchaseWithSupplier.SupplierId.Should().Be(supplier.Id);

        var startPurchaseResponse = await _client.PostAsJsonAsync($"/purchase-orders/{purchaseOrder.Id}/start", new { });
        startPurchaseResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var startedPurchaseOrder = await ReadAsync<PurchaseOrderResponse>(startPurchaseResponse);
        startedPurchaseOrder.Status.Should().Be(PurchaseOrderStatus.Started);

        // Simula chegada do item comprado ao estoque antes da conclusão da compra.
        await SeedStockAsync(part.Id, StockItemType.Part, 10m);

        var completePurchaseResponse = await _client.PostAsJsonAsync($"/purchase-orders/{purchaseOrder.Id}/complete", new { });
        completePurchaseResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var completedPurchaseOrder = await ReadAsync<PurchaseOrderResponse>(completePurchaseResponse);
        completedPurchaseOrder.Status.Should().Be(PurchaseOrderStatus.Completed);
        completedPurchaseOrder.CompletedAt.Should().NotBeNull();

        var separationAfterPurchaseResponse = await _client.GetAsync($"/separation-orders/{separationOrder.Id}");
        separationAfterPurchaseResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var separationAfterPurchase = await ReadAsync<SeparationOrderResponse>(separationAfterPurchaseResponse);
        separationAfterPurchase.Status.Should().Be(SeparationOrderStatus.WaitingPickup);

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
                "Cliente E2E Ruptura",
                CustomerDocumentType.Cpf,
                GenerateValidCpf(),
                $"cliente-ruptura-{Guid.NewGuid():N}@garageflow.test",
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
                "Preto"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return await ReadAsync<VehicleResponse>(response);
    }

    private async Task<PartResponse> CreatePartAsync()
    {
        var response = await _client.PostAsJsonAsync(
            "/parts",
            new CreatePartRequest(
                "Filtro Ruptura E2E",
                UniqueCode("PRT", 10),
                UniqueCode("SKU", 12),
                "UN",
                42m));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return await ReadAsync<PartResponse>(response);
    }

    private async Task<ServiceResponse> CreateServiceAsync()
    {
        var response = await _client.PostAsJsonAsync(
            "/services",
            new CreateServiceRequest(
                UniqueCode("SVC", 12),
                "Serviço E2E Ruptura",
                "Fluxo happy path com compra e retomada",
                210m,
                120));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return await ReadAsync<ServiceResponse>(response);
    }

    private async Task<SupplierResponse> CreateSupplierAsync()
    {
        var index = Interlocked.Increment(ref _supplierSeed);
        var cnpj = SupplierCnpjs[(index - 1) % SupplierCnpjs.Length];

        var response = await _client.PostAsJsonAsync(
            "/suppliers",
            new CreateSupplierRequest(
                "Fornecedor E2E Compra",
                cnpj,
                $"fornecedor-e2e-{index}@garageflow.test",
                "11987654321",
                "Rua das Peças",
                "200",
                null,
                "Industrial",
                "São Paulo",
                "SP",
                "01310100"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return await ReadAsync<SupplierResponse>(response);
    }

    private async Task SeedStockAsync(Guid itemId, StockItemType itemType, decimal quantity)
    {
        var response = await _client.PostAsJsonAsync(
            "/stock/entries",
            new CreateStockEntryRequest(itemId, itemType, quantity, 0m, "Seed E2E ruptura/compra", null));

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
