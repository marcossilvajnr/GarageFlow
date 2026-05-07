using FluentAssertions;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Purchasing;

namespace GarageFlow.Tests.Domain.Purchasing;

public sealed class PurchaseOrderTests
{
    private static PurchaseItem ValidItem(Guid? id = null) =>
        PurchaseItem.Create(id ?? Guid.NewGuid(), PurchaseItemType.Part, "Filtro de óleo", 2m, 15.50m);

    private static PurchaseOrder ValidOrder(IEnumerable<Guid>? separationIds = null, IEnumerable<PurchaseItem>? items = null) =>
        PurchaseOrder.Create(
            separationIds ?? [Guid.NewGuid()],
            items ?? [ValidItem()]);

    // --- PurchaseItem.Create ---

    [Fact]
    public void CreateItem_WithValidData_ReturnsItem()
    {
        var itemId = Guid.NewGuid();
        var item = PurchaseItem.Create(itemId, PurchaseItemType.Supply, "Óleo 5W30", 4m, 12.00m);

        item.ItemId.Should().Be(itemId);
        item.ItemType.Should().Be(PurchaseItemType.Supply);
        item.ItemName.Should().Be("Óleo 5W30");
        item.Quantity.Should().Be(4m);
        item.UnitPrice.Should().Be(12.00m);
        item.Subtotal.Should().Be(48.00m);
    }

    [Fact]
    public void CreateItem_WithEmptyItemId_ThrowsDomainException()
    {
        var act = () => PurchaseItem.Create(Guid.Empty, PurchaseItemType.Part, "Filtro", 1m, 10m);

        act.Should().Throw<DomainException>().WithMessage("Item da ordem de compra inválido");
    }

    [Fact]
    public void CreateItem_WithInvalidItemType_ThrowsDomainException()
    {
        var invalidType = (PurchaseItemType)99;
        var act = () => PurchaseItem.Create(Guid.NewGuid(), invalidType, "Filtro", 1m, 10m);

        act.Should().Throw<DomainException>().WithMessage("Item da ordem de compra inválido");
    }

    [Fact]
    public void CreateItem_WithEmptyName_ThrowsDomainException()
    {
        var act = () => PurchaseItem.Create(Guid.NewGuid(), PurchaseItemType.Part, "   ", 1m, 10m);

        act.Should().Throw<DomainException>().WithMessage("Item da ordem de compra inválido");
    }

    [Fact]
    public void CreateItem_WithZeroQuantity_ThrowsDomainException()
    {
        var act = () => PurchaseItem.Create(Guid.NewGuid(), PurchaseItemType.Part, "Filtro", 0m, 10m);

        act.Should().Throw<DomainException>().WithMessage("Item da ordem de compra inválido");
    }

    [Fact]
    public void CreateItem_WithNegativeUnitPrice_ThrowsDomainException()
    {
        var act = () => PurchaseItem.Create(Guid.NewGuid(), PurchaseItemType.Part, "Filtro", 1m, -1m);

        act.Should().Throw<DomainException>().WithMessage("Item da ordem de compra inválido");
    }

    [Fact]
    public void CreateItem_WithZeroUnitPrice_Succeeds()
    {
        var item = PurchaseItem.Create(Guid.NewGuid(), PurchaseItemType.Part, "Filtro", 2m, 0m);

        item.UnitPrice.Should().Be(0m);
        item.Subtotal.Should().Be(0m);
    }

    [Fact]
    public void CreateItem_TrimsItemName()
    {
        var item = PurchaseItem.Create(Guid.NewGuid(), PurchaseItemType.Part, "  Filtro  ", 1m, 10m);

        item.ItemName.Should().Be("Filtro");
    }

    // --- PurchaseOrder.Create ---

    [Fact]
    public void Create_WithValidData_ReturnsCreatedOrder()
    {
        var separationId = Guid.NewGuid();
        var item = ValidItem();
        var order = PurchaseOrder.Create([separationId], [item]);

        order.Id.Should().NotBeEmpty();
        order.Status.Should().Be(PurchaseOrderStatus.Created);
        order.SeparationOrderIds.Should().ContainSingle().Which.Should().Be(separationId);
        order.Items.Should().HaveCount(1);
        order.SupplierId.Should().BeNull();
        order.StartedAt.Should().BeNull();
        order.CompletedAt.Should().BeNull();
        order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithNoSeparationOrderIds_ThrowsDomainException()
    {
        var act = () => PurchaseOrder.Create([], [ValidItem()]);

        act.Should().Throw<DomainException>().WithMessage("Deve haver pelo menos uma Ordem de Separação");
    }

    [Fact]
    public void Create_WithNullSeparationOrderIds_ThrowsDomainException()
    {
        var act = () => PurchaseOrder.Create(null!, [ValidItem()]);

        act.Should().Throw<DomainException>().WithMessage("Deve haver pelo menos uma Ordem de Separação");
    }

    [Fact]
    public void Create_WithEmptySeparationOrderId_ThrowsDomainException()
    {
        var act = () => PurchaseOrder.Create([Guid.Empty], [ValidItem()]);

        act.Should().Throw<DomainException>().WithMessage("Deve haver pelo menos uma Ordem de Separação");
    }

    [Fact]
    public void Create_WithNoItems_ThrowsDomainException()
    {
        var act = () => PurchaseOrder.Create([Guid.NewGuid()], []);

        act.Should().Throw<DomainException>().WithMessage("Ordem de Compra deve ter pelo menos um item");
    }

    [Fact]
    public void Create_WithNullItems_ThrowsDomainException()
    {
        var act = () => PurchaseOrder.Create([Guid.NewGuid()], null!);

        act.Should().Throw<DomainException>().WithMessage("Ordem de Compra deve ter pelo menos um item");
    }

    [Fact]
    public void Create_WithMultipleSeparationOrders_StoresAll()
    {
        var ids = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var order = PurchaseOrder.Create(ids, [ValidItem()]);

        order.SeparationOrderIds.Should().HaveCount(3);
        order.SeparationOrderIds.Should().BeEquivalentTo(ids);
    }

    // --- AssignSupplier ---

    [Fact]
    public void AssignSupplier_WhenCreated_SetsSupplierId()
    {
        var order = ValidOrder();
        var supplierId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        order.AssignSupplier(supplierId, employeeId);

        order.SupplierId.Should().Be(supplierId);
        order.EmployeeId.Should().Be(employeeId);
    }

    [Fact]
    public void AssignSupplier_WithEmptySupplierId_ThrowsDomainException()
    {
        var order = ValidOrder();

        var act = () => order.AssignSupplier(Guid.Empty, Guid.NewGuid());

        act.Should().Throw<DomainException>().WithMessage("Fornecedor é obrigatório");
    }

    [Fact]
    public void AssignSupplier_AfterStart_ThrowsStatusTransitionException()
    {
        var order = ValidOrder();
        order.AssignSupplier(Guid.NewGuid(), Guid.NewGuid());
        order.Start();

        var act = () => order.AssignSupplier(Guid.NewGuid(), Guid.NewGuid());

        act.Should().Throw<InvalidPurchaseOrderStatusTransitionException>()
            .WithMessage("Não é possível alterar fornecedor após início");
    }

    [Fact]
    public void AssignSupplier_CanBeReassignedWhileCreated()
    {
        var order = ValidOrder();
        var firstSupplier = Guid.NewGuid();
        var secondSupplier = Guid.NewGuid();

        order.AssignSupplier(firstSupplier, Guid.NewGuid());
        order.AssignSupplier(secondSupplier, Guid.NewGuid());

        order.SupplierId.Should().Be(secondSupplier);
    }

    // --- Start ---

    [Fact]
    public void Start_WithSupplierAssigned_SetsStatusStarted()
    {
        var order = ValidOrder();
        order.AssignSupplier(Guid.NewGuid(), Guid.NewGuid());

        order.Start();

        order.Status.Should().Be(PurchaseOrderStatus.Started);
        order.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Start_WithoutSupplier_ThrowsDomainException()
    {
        var order = ValidOrder();

        var act = () => order.Start();

        act.Should().Throw<DomainException>().WithMessage("Fornecedor não foi selecionado");
    }

    [Fact]
    public void Start_WhenAlreadyStarted_ThrowsStatusTransitionException()
    {
        var order = ValidOrder();
        order.AssignSupplier(Guid.NewGuid(), Guid.NewGuid());
        order.Start();

        var act = () => order.Start();

        act.Should().Throw<InvalidPurchaseOrderStatusTransitionException>()
            .WithMessage("Ordem de Compra não está no status Criada");
    }

    [Fact]
    public void Start_WhenCompleted_ThrowsStatusTransitionException()
    {
        var order = ValidOrder();
        order.AssignSupplier(Guid.NewGuid(), Guid.NewGuid());
        order.Start();
        order.Complete();

        var act = () => order.Start();

        act.Should().Throw<InvalidPurchaseOrderStatusTransitionException>()
            .WithMessage("Ordem de Compra não está no status Criada");
    }

    // --- Complete ---

    [Fact]
    public void Complete_WhenStarted_SetsStatusCompleted()
    {
        var order = ValidOrder();
        order.AssignSupplier(Guid.NewGuid(), Guid.NewGuid());
        order.Start();

        order.Complete();

        order.Status.Should().Be(PurchaseOrderStatus.Completed);
        order.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Complete_WhenCreated_ThrowsStatusTransitionException()
    {
        var order = ValidOrder();

        var act = () => order.Complete();

        act.Should().Throw<InvalidPurchaseOrderStatusTransitionException>()
            .WithMessage("Ordem de Compra não está Iniciada");
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_ThrowsStatusTransitionException()
    {
        var order = ValidOrder();
        order.AssignSupplier(Guid.NewGuid(), Guid.NewGuid());
        order.Start();
        order.Complete();

        var act = () => order.Complete();

        act.Should().Throw<InvalidPurchaseOrderStatusTransitionException>()
            .WithMessage("Ordem de Compra não está Iniciada");
    }

    // --- Full flow ---

    [Fact]
    public void FullFlow_Created_Started_Completed_IsValid()
    {
        var order = ValidOrder();
        var supplierId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        order.Status.Should().Be(PurchaseOrderStatus.Created);

        order.AssignSupplier(supplierId, employeeId);
        order.SupplierId.Should().Be(supplierId);
        order.EmployeeId.Should().Be(employeeId);

        order.Start();
        order.Status.Should().Be(PurchaseOrderStatus.Started);
        order.EmployeeId.Should().Be(employeeId);

        order.Complete();
        order.Status.Should().Be(PurchaseOrderStatus.Completed);
        order.EmployeeId.Should().Be(employeeId);
    }
}
