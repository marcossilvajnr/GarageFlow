using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using AppSeparationOrderStatus = GarageFlow.Application.Stock.Enums.SeparationOrderStatus;
using AppStockItemType = GarageFlow.Application.Stock.Enums.StockItemType;
using AppStockOperationType = GarageFlow.Application.Stock.Enums.StockOperationType;
using AppSupplyUnit = GarageFlow.Application.Stock.Enums.SupplyUnit;
using AppCustomerDocumentType = GarageFlow.Application.Customers.Enums.CustomerDocumentType;
using AppEmployeeRole = GarageFlow.Application.Employees.Enums.EmployeeRole;
using GarageFlow.Api.Employees.DTOs;
using GarageFlow.Api.Executions.DTOs;
using GarageFlow.Api.Parts.DTOs;
using GarageFlow.Api.Purchasing.DTOs;
using GarageFlow.Api.Stock.DTOs;
using GarageFlow.Api.Suppliers.DTOs;
using GarageFlow.Domain.Purchasing;
using GarageFlow.Domain.ServiceOrders;
using GarageFlow.Domain.Stock;
using GarageFlow.Infrastructure.Persistence;
using GarageFlow.Tests.Integration;
using Microsoft.Extensions.DependencyInjection;
using AppExecutionOrderStatus = GarageFlow.Application.Executions.Enums.ExecutionOrderStatus;

namespace GarageFlow.Tests.Integration.Purchasing;

public sealed class PurchaseOrderSeparationIntegrationEndpointsTests(GarageFlowWebApplicationFactory factory)
    : IClassFixture<GarageFlowWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static int _supplierSeed = 0;
    private static int _employeeSeed = 0;
    private static int _cpfSeed = 300_000_000;

    private static string NextCnpj()
    {
        var index = Interlocked.Increment(ref _supplierSeed);
        var cnpjs = new[]
        {
            "11.222.333/0001-81", "11.222.333/0002-62", "11.222.333/0003-43",
            "11.222.333/0004-24", "11.222.333/0005-05", "11.222.333/0006-96",
            "11.222.333/0007-77", "11.222.333/0008-58", "11.222.333/0009-39",
            "11.222.333/0010-72"
        };
        return cnpjs[(index - 1) % cnpjs.Length];
    }

    private static string NextSupplierEmail()
    {
        var index = _supplierSeed;
        return $"sep-integ-{index}@fornecedor.com";
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

    private async Task<Guid> CreateSupplier()
    {
        var request = new CreateSupplierRequest(
            "Fornecedor Separação SA",
            NextCnpj(),
            NextSupplierEmail(),
            "11987654321",
            "Rua das Peças",
            "100",
            null,
            "Industrial",
            "São Paulo",
            "SP",
            "01310100");

        var response = await _client.PostAsJsonAsync("/suppliers", request);
        response.EnsureSuccessStatusCode();
        var supplier = (await response.Content.ReadFromJsonAsync<SupplierResponse>(JsonOptions))!;
        return supplier.Id;
    }

    private async Task<Guid> CreateEmployee(AppEmployeeRole role)
    {
        var seed = Interlocked.Increment(ref _employeeSeed);
        var request = new CreateEmployeeRequest(
            $"Employee Chain {seed}",
            AppCustomerDocumentType.Cpf,
            GenerateValidCpf(),
            $"chain-employee-{seed}@garageflow.test",
            $"1196{seed % 1_0000:D4}321",
            "Rua Cadeia",
            "80",
            null,
            "Centro",
            "Sao Paulo",
            "SP",
            "01310100",
            role);

        var response = await _client.PostAsJsonAsync("/employees", request);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<EmployeeResponse>(JsonOptions);
        return body!.Id;
    }

    private async Task<SeparationOrderResponse> CreateSeparationOrderInWaitingPurchase()
    {
        var execution = await CreateExecutionOrder();
        return await CreateSeparationOrderInWaitingPurchase(execution.Id);
    }

    private async Task<SeparationOrderResponse> CreateSeparationOrderInWaitingPurchase(Guid executionOrderId)
    {
        var partId = await CreatePart();
        await CreateStockEntry(partId, AppStockItemType.Part, 100m);

        var createReq = new CreateSeparationOrderRequest(
            executionOrderId,
            [new CreateSeparationPartItemRequest(partId, "Filtro de óleo", 1)],
            []);
        var createResp = await _client.PostAsJsonAsync("/separation-orders", createReq);
        createResp.EnsureSuccessStatusCode();
        var separation = (await createResp.Content.ReadFromJsonAsync<SeparationOrderResponse>(JsonOptions))!;

        var waitResp = await _client.PostAsync($"/separation-orders/{separation.Id}/wait-purchase", null);
        waitResp.EnsureSuccessStatusCode();
        return (await waitResp.Content.ReadFromJsonAsync<SeparationOrderResponse>(JsonOptions))!;
    }

    private async Task<ExecutionOrderResponse> CreateExecutionOrder()
    {
        var serviceOrderId = await SeedServiceOrderApproved();
        var response = await _client.PostAsJsonAsync(
            "/execution-orders",
            new CreateExecutionOrderRequest(serviceOrderId, Guid.NewGuid(), Guid.NewGuid()));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ExecutionOrderResponse>(JsonOptions))!;
    }

    private async Task<Guid> SeedServiceOrderApproved()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GarageFlowDbContext>();
        var so = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        typeof(ServiceOrder)
            .GetProperty(nameof(ServiceOrder.Status))!
            .SetValue(so, ServiceOrderStatus.Approved);
        db.ServiceOrders.Add(so);
        await db.SaveChangesAsync();
        return so.Id;
    }

    private async Task<Guid> CreatePart()
    {
        var request = new CreatePartRequest("Filtro de óleo", $"P-{Guid.NewGuid():N}"[..10], $"SKU-{Guid.NewGuid():N}"[..12], "UN", 50m);
        var response = await _client.PostAsJsonAsync("/parts", request);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<PartResponse>(JsonOptions);
        return body!.Id;
    }

    private async Task CreateStockEntry(Guid itemId, AppStockItemType itemType, decimal initialQuantity)
    {
        var response = await _client.PostAsJsonAsync(
            "/stock/entries",
            new CreateStockEntryRequest(itemId, itemType, initialQuantity, 0m, "Seed integração cadeia compra-separação-execução", null));
        response.EnsureSuccessStatusCode();
    }

    private async Task<PurchaseOrderResponse> CreateStartedPurchaseOrder(Guid separationOrderId)
    {
        var createReq = new CreatePurchaseOrderRequest(
            [separationOrderId],
            [new CreatePurchaseItemRequest(Guid.NewGuid(), PurchaseItemType.Part, "Filtro de óleo", 1m, 25.00m)]);
        var createResp = await _client.PostAsJsonAsync("/purchase-orders", createReq);
        createResp.EnsureSuccessStatusCode();
        var purchaseOrder = (await createResp.Content.ReadFromJsonAsync<PurchaseOrderResponse>(JsonOptions))!;

        var supplierId = await CreateSupplier();
        var stockistId = await CreateEmployee(AppEmployeeRole.Stockist);
        await _client.PostAsJsonAsync(
            $"/purchase-orders/{purchaseOrder.Id}/assign-supplier",
            new AssignPurchaseOrderSupplierRequest(supplierId, stockistId));
        await _client.PostAsync($"/purchase-orders/{purchaseOrder.Id}/start", null);

        return purchaseOrder;
    }

    // --- Fluxo feliz: concluir compra retoma separações em WaitingPurchase ---

    [Fact]
    public async Task CompletePurchaseOrder_WithSeparationInWaitingPurchase_Returns200()
    {
        var separation = await CreateSeparationOrderInWaitingPurchase();
        var purchaseOrder = await CreateStartedPurchaseOrder(separation.Id);
        var stockistId = await CreateEmployee(AppEmployeeRole.Stockist);

        var response = await _client.PostAsync($"/purchase-orders/{purchaseOrder.Id}/complete", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PurchaseOrderResponse>(JsonOptions);
        body!.Status.Should().Be(PurchaseOrderStatus.Completed);
        body.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CompletePurchaseOrder_SeparationMovesToWaitingPickup()
    {
        var separation = await CreateSeparationOrderInWaitingPurchase();
        var purchaseOrder = await CreateStartedPurchaseOrder(separation.Id);
        var stockistId = await CreateEmployee(AppEmployeeRole.Stockist);

        await _client.PostAsync($"/purchase-orders/{purchaseOrder.Id}/complete", null);

        var separationResp = await _client.GetAsync($"/separation-orders/{separation.Id}");
        separationResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var separationBody = await separationResp.Content.ReadFromJsonAsync<SeparationOrderResponse>(JsonOptions);
        separationBody!.Status.Should().Be(AppSeparationOrderStatus.WaitingPickup);
    }

    // --- Separação vinculada inexistente (404) ---

    [Fact]
    public async Task CompletePurchaseOrder_WhenLinkedSeparationNotFound_Returns404()
    {
        var nonExistentSeparationId = Guid.NewGuid();
        var purchaseOrder = await CreateStartedPurchaseOrder(nonExistentSeparationId);
        var stockistId = await CreateEmployee(AppEmployeeRole.Stockist);

        var response = await _client.PostAsync($"/purchase-orders/{purchaseOrder.Id}/complete", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // --- Separação em estado inválido para retomada (409) ---

    [Fact]
    public async Task CompletePurchaseOrder_WhenSeparationInWrongState_Returns409()
    {
        var partId = await CreatePart();
        await CreateStockEntry(partId, AppStockItemType.Part, 100m);

        var createReq = new CreateSeparationOrderRequest(
            Guid.NewGuid(),
            [new CreateSeparationPartItemRequest(partId, "Filtro de óleo", 1)],
            []);
        var createResp = await _client.PostAsJsonAsync("/separation-orders", createReq);
        createResp.EnsureSuccessStatusCode();
        var separation = (await createResp.Content.ReadFromJsonAsync<SeparationOrderResponse>(JsonOptions))!;

        await _client.PostAsync($"/separation-orders/{separation.Id}/reserve", null);

        var purchaseOrder = await CreateStartedPurchaseOrder(separation.Id);
        var stockistId = await CreateEmployee(AppEmployeeRole.Stockist);

        var response = await _client.PostAsync($"/purchase-orders/{purchaseOrder.Id}/complete", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // --- Transição inválida da purchase order (409) ---

    [Fact]
    public async Task CompletePurchaseOrder_WhenPurchaseOrderNotStarted_Returns409()
    {
        var separation = await CreateSeparationOrderInWaitingPurchase();
        var createReq = new CreatePurchaseOrderRequest(
            [separation.Id],
            [new CreatePurchaseItemRequest(Guid.NewGuid(), PurchaseItemType.Part, "Filtro", 1m, 10m)]);
        var createResp = await _client.PostAsJsonAsync("/purchase-orders", createReq);
        createResp.EnsureSuccessStatusCode();
        var purchaseOrder = (await createResp.Content.ReadFromJsonAsync<PurchaseOrderResponse>(JsonOptions))!;
        var stockistId = await CreateEmployee(AppEmployeeRole.Stockist);

        var response = await _client.PostAsync($"/purchase-orders/{purchaseOrder.Id}/complete", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // --- Purchase order inexistente (404) ---

    [Fact]
    public async Task CompletePurchaseOrder_WhenPurchaseOrderNotFound_Returns404()
    {
        var stockistId = await CreateEmployee(AppEmployeeRole.Stockist);
        var response = await _client.PostAsync($"/purchase-orders/{Guid.NewGuid()}/complete", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CompletePurchaseOrder_ChainToExecution_StartBeforeMechanicReceipt_Returns409()
    {
        var execution = await CreateExecutionOrder();
        var separation = await CreateSeparationOrderInWaitingPurchase(execution.Id);
        var purchaseOrder = await CreateStartedPurchaseOrder(separation.Id);
        var stockistId = await CreateEmployee(AppEmployeeRole.Stockist);
        var mechanicId = await CreateEmployee(AppEmployeeRole.Mechanic);

        var completePurchaseResponse = await _client.PostAsync($"/purchase-orders/{purchaseOrder.Id}/complete", null);
        completePurchaseResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var startExecutionResponse = await _client.PostAsync(
            $"/execution-orders/{execution.Id}/start", null);

        startExecutionResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CompletePurchaseOrder_ChainToExecution_AfterCustodyExecutionBecomesReadyAndCanStart()
    {
        var execution = await CreateExecutionOrder();
        var separation = await CreateSeparationOrderInWaitingPurchase(execution.Id);
        var purchaseOrder = await CreateStartedPurchaseOrder(separation.Id);
        var stockistId = await CreateEmployee(AppEmployeeRole.Stockist);
        var secondStockistId = await CreateEmployee(AppEmployeeRole.Stockist);
        var mechanicId = await CreateEmployee(AppEmployeeRole.Mechanic);

        var completePurchaseResponse = await _client.PostAsync($"/purchase-orders/{purchaseOrder.Id}/complete", null);
        completePurchaseResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var confirmStockistResponse = await _client.PostAsJsonAsync(
            $"/separation-orders/{separation.Id}/confirm-stockist-withdrawal",
            new ConfirmSeparationStockistWithdrawalRequest(secondStockistId));
        confirmStockistResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var confirmMechanicResponse = await _client.PostAsync(
            $"/separation-orders/{separation.Id}/confirm-mechanic-receipt",
            null);
        confirmMechanicResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var executionGetResponse = await _client.GetAsync($"/execution-orders/{execution.Id}");
        executionGetResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var executionBody = await executionGetResponse.Content.ReadFromJsonAsync<ExecutionOrderResponse>(JsonOptions);
        executionBody!.Status.Should().Be(AppExecutionOrderStatus.Ready);

        var startExecutionResponse = await _client.PostAsync(
            $"/execution-orders/{execution.Id}/start", null);
        startExecutionResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
