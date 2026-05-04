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

public sealed class PurchaseOrdersEndpointsTests(GarageFlowWebApplicationFactory factory)
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
        // Mathematically valid CNPJs for testing (aligned with supplier suite)
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
        return $"compras-{index}@fornecedor.com";
    }

    private static CreatePurchaseOrderRequest ValidCreateRequest(IReadOnlyList<Guid>? separationIds = null) =>
        new(
            separationIds ?? [Guid.NewGuid()],
            [new CreatePurchaseItemRequest(Guid.NewGuid(), PurchaseItemType.Part, "Filtro de óleo", 2m, 15.50m)]);

    private async Task<PurchaseOrderResponse> CreatePurchaseOrder(CreatePurchaseOrderRequest? request = null)
    {
        var req = request ?? ValidCreateRequest();
        var response = await _client.PostAsJsonAsync("/purchase-orders", req);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PurchaseOrderResponse>(JsonOptions))!;
    }

    private async Task<Guid> CreateSupplier()
    {
        var request = new CreateSupplierRequest(
            "Fornecedor Compras SA",
            NextCnpj(),
            NextSupplierEmail(),
            "11987654321",
            "Rua das Compras",
            "200",
            null,
            "Centro",
            "São Paulo",
            "SP",
            "01310100");

        var response = await _client.PostAsJsonAsync("/suppliers", request);
        response.EnsureSuccessStatusCode();
        var supplier = (await response.Content.ReadFromJsonAsync<SupplierResponse>(JsonOptions))!;
        return supplier.Id;
    }

    // --- POST /purchase-orders ---

    [Fact]
    public async Task CreatePurchaseOrder_WithValidData_Returns201()
    {
        var response = await _client.PostAsJsonAsync("/purchase-orders", ValidCreateRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<PurchaseOrderResponse>(JsonOptions);
        body.Should().NotBeNull();
        body!.Status.Should().Be(PurchaseOrderStatus.Created);
        body.SeparationOrderIds.Should().HaveCount(1);
        body.Items.Should().HaveCount(1);
        body.SupplierId.Should().BeNull();
    }

    [Fact]
    public async Task CreatePurchaseOrder_WithNoSeparationOrderIds_Returns400()
    {
        var request = new CreatePurchaseOrderRequest(
            [],
            [new CreatePurchaseItemRequest(Guid.NewGuid(), PurchaseItemType.Part, "Filtro", 1m, 10m)]);

        var response = await _client.PostAsJsonAsync("/purchase-orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreatePurchaseOrder_WithNullSeparationOrderIds_Returns400()
    {
        var request = new CreatePurchaseOrderRequest(null, [new CreatePurchaseItemRequest(Guid.NewGuid(), PurchaseItemType.Part, "Filtro", 1m, 10m)]);

        var response = await _client.PostAsJsonAsync("/purchase-orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreatePurchaseOrder_WithNoItems_Returns400()
    {
        var request = new CreatePurchaseOrderRequest([Guid.NewGuid()], []);

        var response = await _client.PostAsJsonAsync("/purchase-orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreatePurchaseOrder_WithInvalidItem_Returns400()
    {
        var request = new CreatePurchaseOrderRequest(
            [Guid.NewGuid()],
            [new CreatePurchaseItemRequest(Guid.Empty, PurchaseItemType.Part, "Filtro", 1m, 10m)]);

        var response = await _client.PostAsJsonAsync("/purchase-orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreatePurchaseOrder_WithInvalidItemType_Returns400()
    {
        var invalidType = (PurchaseItemType)99;
        var request = new CreatePurchaseOrderRequest(
            [Guid.NewGuid()],
            [new CreatePurchaseItemRequest(Guid.NewGuid(), invalidType, "Filtro", 1m, 10m)]);

        var response = await _client.PostAsJsonAsync("/purchase-orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // --- GET /purchase-orders/{id} ---

    [Fact]
    public async Task GetPurchaseOrderById_WhenExists_Returns200()
    {
        var created = await CreatePurchaseOrder();

        var response = await _client.GetAsync($"/purchase-orders/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PurchaseOrderResponse>(JsonOptions);
        body!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetPurchaseOrderById_WhenNotExists_Returns404()
    {
        var response = await _client.GetAsync($"/purchase-orders/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // --- GET /purchase-orders ---

    [Fact]
    public async Task ListPurchaseOrders_Returns200()
    {
        var response = await _client.GetAsync("/purchase-orders");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ListPurchaseOrders_WithInvalidPage_Returns400()
    {
        var response = await _client.GetAsync("/purchase-orders?page=0");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // --- POST /purchase-orders/{id}/assign-supplier ---

    [Fact]
    public async Task AssignSupplier_WithValidSupplier_Returns200()
    {
        var created = await CreatePurchaseOrder();
        var supplierId = await CreateSupplier();

        var request = new AssignPurchaseOrderSupplierRequest(supplierId);
        var response = await _client.PostAsJsonAsync($"/purchase-orders/{created.Id}/assign-supplier", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PurchaseOrderResponse>(JsonOptions);
        body!.SupplierId.Should().Be(supplierId);
    }

    [Fact]
    public async Task AssignSupplier_WithNonExistentOrder_Returns404()
    {
        var supplierId = await CreateSupplier();
        var request = new AssignPurchaseOrderSupplierRequest(supplierId);

        var response = await _client.PostAsJsonAsync($"/purchase-orders/{Guid.NewGuid()}/assign-supplier", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AssignSupplier_WithNonExistentSupplier_Returns400()
    {
        var created = await CreatePurchaseOrder();
        var request = new AssignPurchaseOrderSupplierRequest(Guid.NewGuid());

        var response = await _client.PostAsJsonAsync($"/purchase-orders/{created.Id}/assign-supplier", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AssignSupplier_AfterStart_Returns409()
    {
        var created = await CreatePurchaseOrder();
        var supplierId = await CreateSupplier();

        var assignRequest = new AssignPurchaseOrderSupplierRequest(supplierId);
        await _client.PostAsJsonAsync($"/purchase-orders/{created.Id}/assign-supplier", assignRequest);
        await _client.PostAsJsonAsync($"/purchase-orders/{created.Id}/start", new { });

        var response = await _client.PostAsJsonAsync($"/purchase-orders/{created.Id}/assign-supplier", assignRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // --- POST /purchase-orders/{id}/start ---

    [Fact]
    public async Task StartPurchaseOrder_WithSupplierAssigned_Returns200()
    {
        var created = await CreatePurchaseOrder();
        var supplierId = await CreateSupplier();

        await _client.PostAsJsonAsync($"/purchase-orders/{created.Id}/assign-supplier",
            new AssignPurchaseOrderSupplierRequest(supplierId));

        var response = await _client.PostAsJsonAsync($"/purchase-orders/{created.Id}/start", new { });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PurchaseOrderResponse>(JsonOptions);
        body!.Status.Should().Be(PurchaseOrderStatus.Started);
    }

    [Fact]
    public async Task StartPurchaseOrder_WithoutSupplier_Returns400()
    {
        var created = await CreatePurchaseOrder();

        var response = await _client.PostAsJsonAsync($"/purchase-orders/{created.Id}/start", new { });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task StartPurchaseOrder_WhenNotFound_Returns404()
    {
        var response = await _client.PostAsJsonAsync($"/purchase-orders/{Guid.NewGuid()}/start", new { });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task StartPurchaseOrder_WhenAlreadyStarted_Returns409()
    {
        var created = await CreatePurchaseOrder();
        var supplierId = await CreateSupplier();

        await _client.PostAsJsonAsync($"/purchase-orders/{created.Id}/assign-supplier",
            new AssignPurchaseOrderSupplierRequest(supplierId));
        await _client.PostAsJsonAsync($"/purchase-orders/{created.Id}/start", new { });

        var response = await _client.PostAsJsonAsync($"/purchase-orders/{created.Id}/start", new { });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
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

    // --- POST /purchase-orders/{id}/complete ---

    [Fact]
    public async Task CompletePurchaseOrder_WhenStarted_Returns200()
    {
        var separation = await CreateSeparationOrderInWaitingPurchase();
        var created = await CreatePurchaseOrder(ValidCreateRequest([separation.Id]));
        var supplierId = await CreateSupplier();

        await _client.PostAsJsonAsync($"/purchase-orders/{created.Id}/assign-supplier",
            new AssignPurchaseOrderSupplierRequest(supplierId));
        await _client.PostAsJsonAsync($"/purchase-orders/{created.Id}/start", new { });

        var response = await _client.PostAsJsonAsync($"/purchase-orders/{created.Id}/complete", new { });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PurchaseOrderResponse>(JsonOptions);
        body!.Status.Should().Be(PurchaseOrderStatus.Completed);
        body.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CompletePurchaseOrder_SeparationMovesToWaitingPickup()
    {
        var separation = await CreateSeparationOrderInWaitingPurchase();
        var created = await CreatePurchaseOrder(ValidCreateRequest([separation.Id]));
        var supplierId = await CreateSupplier();

        await _client.PostAsJsonAsync($"/purchase-orders/{created.Id}/assign-supplier",
            new AssignPurchaseOrderSupplierRequest(supplierId));
        await _client.PostAsJsonAsync($"/purchase-orders/{created.Id}/start", new { });
        await _client.PostAsJsonAsync($"/purchase-orders/{created.Id}/complete", new { });

        var separationResp = await _client.GetAsync($"/separation-orders/{separation.Id}");
        separationResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var separationBody = await separationResp.Content.ReadFromJsonAsync<SeparationOrderResponse>(JsonOptions);
        separationBody!.Status.Should().Be(SeparationOrderStatus.WaitingPickup);
    }

    [Fact]
    public async Task CompletePurchaseOrder_WhenCreated_Returns409()
    {
        var separation = await CreateSeparationOrderInWaitingPurchase();
        var created = await CreatePurchaseOrder(ValidCreateRequest([separation.Id]));

        var response = await _client.PostAsJsonAsync($"/purchase-orders/{created.Id}/complete", new { });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CompletePurchaseOrder_WhenNotFound_Returns404()
    {
        var response = await _client.PostAsJsonAsync($"/purchase-orders/{Guid.NewGuid()}/complete", new { });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
