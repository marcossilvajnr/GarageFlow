using FluentAssertions;
using GarageFlow.Application.Purchasing.Commands;
using GarageFlow.Application.Purchasing.Handlers;
using GarageFlow.Application.Purchasing.Queries;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Purchasing;
using GarageFlow.Domain.Stock;
using GarageFlow.Domain.Suppliers;
using GarageFlow.Domain.ValueObjects;
using GarageFlow.Tests.Application.Stock;
using GarageFlow.Tests.Application.Suppliers;

namespace GarageFlow.Tests.Application.Purchasing;

public sealed class PurchaseOrderHandlersTests
{
    private static CreatePurchaseOrderCommand ValidCreateCommand(IReadOnlyList<Guid>? separationIds = null) =>
        new(
            separationIds ?? [Guid.NewGuid()],
            [new CreatePurchaseItemCommand(Guid.NewGuid(), PurchaseItemType.Part, "Filtro de óleo", 2m, 15.50m)]);

    private static Supplier ValidSupplier()
    {
        var address = Address.Create("Rua Teste", "123", null, "Centro", "São Paulo", "SP", "01310-100");
        return Supplier.Create("Fornecedor Teste", "11222333000181", "contato@fornecedor.com", "(11) 99999-9999", address);
    }

    // --- CreatePurchaseOrderHandler ---

    [Fact]
    public async Task CreatePurchaseOrder_WithValidData_ReturnsDtoWithStatusCreated()
    {
        var repo = new FakePurchaseOrderRepository();
        var handler = new CreatePurchaseOrderHandler(repo);
        var command = ValidCreateCommand();

        var dto = await handler.HandleAsync(command);

        dto.Should().NotBeNull();
        dto.Id.Should().NotBeEmpty();
        dto.Status.Should().Be(PurchaseOrderStatus.Created);
        dto.SeparationOrderIds.Should().HaveCount(1);
        dto.Items.Should().HaveCount(1);
        dto.SupplierId.Should().BeNull();
    }

    [Fact]
    public async Task CreatePurchaseOrder_WithNoSeparationOrderIds_ThrowsDomainException()
    {
        var repo = new FakePurchaseOrderRepository();
        var handler = new CreatePurchaseOrderHandler(repo);
        var command = new CreatePurchaseOrderCommand(
            [],
            [new CreatePurchaseItemCommand(Guid.NewGuid(), PurchaseItemType.Part, "Filtro", 1m, 10m)]);

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<DomainException>().WithMessage("Deve haver pelo menos uma Ordem de Separação");
    }

    [Fact]
    public async Task CreatePurchaseOrder_WithNoItems_ThrowsDomainException()
    {
        var repo = new FakePurchaseOrderRepository();
        var handler = new CreatePurchaseOrderHandler(repo);
        var command = new CreatePurchaseOrderCommand([Guid.NewGuid()], []);

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<DomainException>().WithMessage("Ordem de Compra deve ter pelo menos um item");
    }

    [Fact]
    public async Task CreatePurchaseOrder_WithInvalidItem_ThrowsDomainException()
    {
        var repo = new FakePurchaseOrderRepository();
        var handler = new CreatePurchaseOrderHandler(repo);
        var command = new CreatePurchaseOrderCommand(
            [Guid.NewGuid()],
            [new CreatePurchaseItemCommand(Guid.Empty, PurchaseItemType.Part, "Filtro", 1m, 10m)]);

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<DomainException>().WithMessage("Item da ordem de compra inválido");
    }

    // --- GetPurchaseOrderByIdHandler ---

    [Fact]
    public async Task GetPurchaseOrderById_WhenExists_ReturnsDto()
    {
        var repo = new FakePurchaseOrderRepository();
        var createHandler = new CreatePurchaseOrderHandler(repo);
        var dto = await createHandler.HandleAsync(ValidCreateCommand());

        var getHandler = new GetPurchaseOrderByIdHandler(repo);
        var result = await getHandler.HandleAsync(new GetPurchaseOrderByIdQuery(dto.Id));

        result.Should().NotBeNull();
        result.Id.Should().Be(dto.Id);
    }

    [Fact]
    public async Task GetPurchaseOrderById_WhenNotExists_ThrowsEntityNotFoundException()
    {
        var repo = new FakePurchaseOrderRepository();
        var handler = new GetPurchaseOrderByIdHandler(repo);

        var act = async () => await handler.HandleAsync(new GetPurchaseOrderByIdQuery(Guid.NewGuid()));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    // --- AssignPurchaseOrderSupplierHandler ---

    [Fact]
    public async Task AssignSupplier_WithValidSupplier_SetsSupplierId()
    {
        var purchaseRepo = new FakePurchaseOrderRepository();
        var supplierRepo = new FakeSupplierRepository();
        var supplier = ValidSupplier();
        await supplierRepo.AddAsync(supplier);

        var createHandler = new CreatePurchaseOrderHandler(purchaseRepo);
        var dto = await createHandler.HandleAsync(ValidCreateCommand());

        var assignHandler = new AssignPurchaseOrderSupplierHandler(purchaseRepo, supplierRepo);
        var result = await assignHandler.HandleAsync(new AssignPurchaseOrderSupplierCommand(dto.Id, supplier.Id));

        result.SupplierId.Should().Be(supplier.Id);
    }

    [Fact]
    public async Task AssignSupplier_WithNonExistentSupplier_ThrowsDomainException()
    {
        var purchaseRepo = new FakePurchaseOrderRepository();
        var supplierRepo = new FakeSupplierRepository();

        var createHandler = new CreatePurchaseOrderHandler(purchaseRepo);
        var dto = await createHandler.HandleAsync(ValidCreateCommand());

        var assignHandler = new AssignPurchaseOrderSupplierHandler(purchaseRepo, supplierRepo);
        var act = async () => await assignHandler.HandleAsync(
            new AssignPurchaseOrderSupplierCommand(dto.Id, Guid.NewGuid()));

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task AssignSupplier_WhenOrderNotFound_ThrowsEntityNotFoundException()
    {
        var purchaseRepo = new FakePurchaseOrderRepository();
        var supplierRepo = new FakeSupplierRepository();

        var handler = new AssignPurchaseOrderSupplierHandler(purchaseRepo, supplierRepo);
        var act = async () => await handler.HandleAsync(
            new AssignPurchaseOrderSupplierCommand(Guid.NewGuid(), Guid.NewGuid()));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task AssignSupplier_AfterStart_ThrowsStatusTransitionException()
    {
        var purchaseRepo = new FakePurchaseOrderRepository();
        var supplierRepo = new FakeSupplierRepository();
        var supplier = ValidSupplier();
        await supplierRepo.AddAsync(supplier);

        var createHandler = new CreatePurchaseOrderHandler(purchaseRepo);
        var dto = await createHandler.HandleAsync(ValidCreateCommand());

        var assignHandler = new AssignPurchaseOrderSupplierHandler(purchaseRepo, supplierRepo);
        await assignHandler.HandleAsync(new AssignPurchaseOrderSupplierCommand(dto.Id, supplier.Id));

        var startHandler = new StartPurchaseOrderHandler(purchaseRepo);
        await startHandler.HandleAsync(new StartPurchaseOrderCommand(dto.Id));

        var act = async () => await assignHandler.HandleAsync(
            new AssignPurchaseOrderSupplierCommand(dto.Id, supplier.Id));

        await act.Should().ThrowAsync<InvalidPurchaseOrderStatusTransitionException>();
    }

    // --- StartPurchaseOrderHandler ---

    [Fact]
    public async Task Start_WithSupplierAssigned_ReturnsStartedStatus()
    {
        var purchaseRepo = new FakePurchaseOrderRepository();
        var supplierRepo = new FakeSupplierRepository();
        var supplier = ValidSupplier();
        await supplierRepo.AddAsync(supplier);

        var createHandler = new CreatePurchaseOrderHandler(purchaseRepo);
        var dto = await createHandler.HandleAsync(ValidCreateCommand());

        var assignHandler = new AssignPurchaseOrderSupplierHandler(purchaseRepo, supplierRepo);
        await assignHandler.HandleAsync(new AssignPurchaseOrderSupplierCommand(dto.Id, supplier.Id));

        var startHandler = new StartPurchaseOrderHandler(purchaseRepo);
        var result = await startHandler.HandleAsync(new StartPurchaseOrderCommand(dto.Id));

        result.Status.Should().Be(PurchaseOrderStatus.Started);
        result.StartedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Start_WithoutSupplier_ThrowsDomainException()
    {
        var purchaseRepo = new FakePurchaseOrderRepository();
        var createHandler = new CreatePurchaseOrderHandler(purchaseRepo);
        var dto = await createHandler.HandleAsync(ValidCreateCommand());

        var startHandler = new StartPurchaseOrderHandler(purchaseRepo);
        var act = async () => await startHandler.HandleAsync(new StartPurchaseOrderCommand(dto.Id));

        await act.Should().ThrowAsync<DomainException>().WithMessage("Fornecedor não foi selecionado");
    }

    [Fact]
    public async Task Start_WhenNotFound_ThrowsEntityNotFoundException()
    {
        var purchaseRepo = new FakePurchaseOrderRepository();
        var handler = new StartPurchaseOrderHandler(purchaseRepo);

        var act = async () => await handler.HandleAsync(new StartPurchaseOrderCommand(Guid.NewGuid()));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Start_WhenAlreadyStarted_ThrowsStatusTransitionException()
    {
        var purchaseRepo = new FakePurchaseOrderRepository();
        var supplierRepo = new FakeSupplierRepository();
        var supplier = ValidSupplier();
        await supplierRepo.AddAsync(supplier);

        var createHandler = new CreatePurchaseOrderHandler(purchaseRepo);
        var dto = await createHandler.HandleAsync(ValidCreateCommand());

        var assignHandler = new AssignPurchaseOrderSupplierHandler(purchaseRepo, supplierRepo);
        await assignHandler.HandleAsync(new AssignPurchaseOrderSupplierCommand(dto.Id, supplier.Id));

        var startHandler = new StartPurchaseOrderHandler(purchaseRepo);
        await startHandler.HandleAsync(new StartPurchaseOrderCommand(dto.Id));

        var act = async () => await startHandler.HandleAsync(new StartPurchaseOrderCommand(dto.Id));

        await act.Should().ThrowAsync<InvalidPurchaseOrderStatusTransitionException>();
    }

    // --- CompletePurchaseOrderHandler ---

    private static SeparationOrder SeparationOrderInWaitingPurchase()
    {
        var separationOrder = SeparationOrder.Create(
            Guid.NewGuid(),
            [SeparationPartItem.Create(Guid.NewGuid(), "Filtro de óleo", 1)],
            []);
        separationOrder.WaitForPurchase();
        return separationOrder;
    }

    [Fact]
    public async Task Complete_WhenStarted_ReturnsCompletedStatus()
    {
        var purchaseRepo = new FakePurchaseOrderRepository();
        var separationRepo = new FakeSeparationOrderRepository();
        var supplierRepo = new FakeSupplierRepository();
        var supplier = ValidSupplier();
        await supplierRepo.AddAsync(supplier);

        var separationOrder = SeparationOrderInWaitingPurchase();
        await separationRepo.AddAsync(separationOrder);

        var createHandler = new CreatePurchaseOrderHandler(purchaseRepo);
        var dto = await createHandler.HandleAsync(ValidCreateCommand([separationOrder.Id]));

        var assignHandler = new AssignPurchaseOrderSupplierHandler(purchaseRepo, supplierRepo);
        await assignHandler.HandleAsync(new AssignPurchaseOrderSupplierCommand(dto.Id, supplier.Id));

        var startHandler = new StartPurchaseOrderHandler(purchaseRepo);
        await startHandler.HandleAsync(new StartPurchaseOrderCommand(dto.Id));

        var completeHandler = new CompletePurchaseOrderHandler(purchaseRepo, separationRepo);
        var result = await completeHandler.HandleAsync(new CompletePurchaseOrderCommand(dto.Id));

        result.Status.Should().Be(PurchaseOrderStatus.Completed);
        result.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Complete_WhenCreated_ThrowsStatusTransitionException()
    {
        var purchaseRepo = new FakePurchaseOrderRepository();
        var separationRepo = new FakeSeparationOrderRepository();
        var createHandler = new CreatePurchaseOrderHandler(purchaseRepo);
        var dto = await createHandler.HandleAsync(ValidCreateCommand());

        var completeHandler = new CompletePurchaseOrderHandler(purchaseRepo, separationRepo);
        var act = async () => await completeHandler.HandleAsync(new CompletePurchaseOrderCommand(dto.Id));

        await act.Should().ThrowAsync<InvalidPurchaseOrderStatusTransitionException>()
            .WithMessage("Ordem de Compra não está Iniciada");
    }

    [Fact]
    public async Task Complete_WhenNotFound_ThrowsEntityNotFoundException()
    {
        var purchaseRepo = new FakePurchaseOrderRepository();
        var separationRepo = new FakeSeparationOrderRepository();
        var handler = new CompletePurchaseOrderHandler(purchaseRepo, separationRepo);

        var act = async () => await handler.HandleAsync(new CompletePurchaseOrderCommand(Guid.NewGuid()));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    // --- ListPurchaseOrdersHandler ---

    [Fact]
    public async Task ListPurchaseOrders_ReturnsPagedResult()
    {
        var repo = new FakePurchaseOrderRepository();
        var createHandler = new CreatePurchaseOrderHandler(repo);
        await createHandler.HandleAsync(ValidCreateCommand());
        await createHandler.HandleAsync(ValidCreateCommand());

        var listHandler = new ListPurchaseOrdersHandler(repo);
        var result = await listHandler.HandleAsync(new ListPurchaseOrdersQuery(1, 10));

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }
}
