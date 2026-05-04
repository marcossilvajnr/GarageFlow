using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using GarageFlow.Api.DTOs.Purchasing;
using GarageFlow.Api.DTOs.Stock;
using GarageFlow.Api.DTOs.Suppliers;
using GarageFlow.Domain.Purchasing;
using GarageFlow.Domain.Stock;
using GarageFlow.Tests.Integration;

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

    private async Task<SeparationOrderResponse> CreateSeparationOrderInWaitingPurchase()
    {
        var createReq = new CreateSeparationOrderRequest(
            Guid.NewGuid(),
            [new CreateSeparationPartItemRequest(Guid.NewGuid(), "Filtro de óleo", 1)],
            []);
        var createResp = await _client.PostAsJsonAsync("/separation-orders", createReq);
        createResp.EnsureSuccessStatusCode();
        var separation = (await createResp.Content.ReadFromJsonAsync<SeparationOrderResponse>(JsonOptions))!;

        var waitResp = await _client.PostAsync($"/separation-orders/{separation.Id}/wait-purchase", null);
        waitResp.EnsureSuccessStatusCode();
        return (await waitResp.Content.ReadFromJsonAsync<SeparationOrderResponse>(JsonOptions))!;
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
        await _client.PostAsJsonAsync(
            $"/purchase-orders/{purchaseOrder.Id}/assign-supplier",
            new AssignPurchaseOrderSupplierRequest(supplierId));
        await _client.PostAsJsonAsync($"/purchase-orders/{purchaseOrder.Id}/start", new { });

        return purchaseOrder;
    }

    // --- Fluxo feliz: concluir compra retoma separações em WaitingPurchase ---

    [Fact]
    public async Task CompletePurchaseOrder_WithSeparationInWaitingPurchase_Returns200()
    {
        var separation = await CreateSeparationOrderInWaitingPurchase();
        var purchaseOrder = await CreateStartedPurchaseOrder(separation.Id);

        var response = await _client.PostAsJsonAsync($"/purchase-orders/{purchaseOrder.Id}/complete", new { });

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

        await _client.PostAsJsonAsync($"/purchase-orders/{purchaseOrder.Id}/complete", new { });

        var separationResp = await _client.GetAsync($"/separation-orders/{separation.Id}");
        separationResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var separationBody = await separationResp.Content.ReadFromJsonAsync<SeparationOrderResponse>(JsonOptions);
        separationBody!.Status.Should().Be(SeparationOrderStatus.WaitingPickup);
    }

    // --- Separação vinculada inexistente (404) ---

    [Fact]
    public async Task CompletePurchaseOrder_WhenLinkedSeparationNotFound_Returns404()
    {
        var nonExistentSeparationId = Guid.NewGuid();
        var purchaseOrder = await CreateStartedPurchaseOrder(nonExistentSeparationId);

        var response = await _client.PostAsJsonAsync($"/purchase-orders/{purchaseOrder.Id}/complete", new { });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // --- Separação em estado inválido para retomada (409) ---

    [Fact]
    public async Task CompletePurchaseOrder_WhenSeparationInWrongState_Returns409()
    {
        var createReq = new CreateSeparationOrderRequest(
            Guid.NewGuid(),
            [new CreateSeparationPartItemRequest(Guid.NewGuid(), "Filtro de óleo", 1)],
            []);
        var createResp = await _client.PostAsJsonAsync("/separation-orders", createReq);
        createResp.EnsureSuccessStatusCode();
        var separation = (await createResp.Content.ReadFromJsonAsync<SeparationOrderResponse>(JsonOptions))!;

        await _client.PostAsync($"/separation-orders/{separation.Id}/reserve", null);

        var purchaseOrder = await CreateStartedPurchaseOrder(separation.Id);

        var response = await _client.PostAsJsonAsync($"/purchase-orders/{purchaseOrder.Id}/complete", new { });

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

        var response = await _client.PostAsJsonAsync($"/purchase-orders/{purchaseOrder.Id}/complete", new { });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // --- Purchase order inexistente (404) ---

    [Fact]
    public async Task CompletePurchaseOrder_WhenPurchaseOrderNotFound_Returns404()
    {
        var response = await _client.PostAsJsonAsync($"/purchase-orders/{Guid.NewGuid()}/complete", new { });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
