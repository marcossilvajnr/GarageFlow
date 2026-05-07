using FluentAssertions;
using GarageFlow.Application.Purchasing.Commands;
using GarageFlow.Application.Purchasing.Handlers;
using GarageFlow.Domain.Employees;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Purchasing;
using GarageFlow.Domain.Stock;
using GarageFlow.Domain.Suppliers;
using GarageFlow.Domain.ValueObjects;
using GarageFlow.Tests.Application.Employees;
using GarageFlow.Tests.Application.Stock;
using GarageFlow.Tests.Application.Suppliers;

namespace GarageFlow.Tests.Application.Purchasing;

public sealed class PurchaseOrderSeparationIntegrationTests
{
    private static Supplier ValidSupplier()
    {
        var address = Address.Create("Rua Teste", "123", null, "Centro", "São Paulo", "SP", "01310-100");
        return Supplier.Create("Fornecedor Teste", "11222333000181", "contato@fornecedor.com", "(11) 99999-9999", address);
    }

    private static CreatePurchaseOrderCommand CreateCommandForSeparations(IReadOnlyList<Guid> separationIds) =>
        new(
            separationIds,
            [new CreatePurchaseItemCommand(Guid.NewGuid(), PurchaseItemType.Part, "Filtro de óleo", 2m, 15.50m)]);

    private static async Task<Employee> SeedEmployeeAsync(
        FakeEmployeeRepository employeeRepo,
        EmployeeRole role = EmployeeRole.Stockist)
    {
        var address = Address.Create("Rua Teste", "123", null, "Centro", "São Paulo", "SP", "01310-100");
        var employee = Employee.Create(
            "Funcionário Teste",
            GarageFlow.Domain.Customers.CustomerDocumentType.Cpf,
            "529.982.247-25",
            "funcionario@garageflow.com",
            "11987654321",
            address,
            role);

        await employeeRepo.AddAsync(employee);
        return employee;
    }

    private static SeparationOrder SeparationOrderInWaitingPurchase()
    {
        var separationOrder = SeparationOrder.Create(
            Guid.NewGuid(),
            [SeparationPartItem.Create(Guid.NewGuid(), "Filtro de óleo", 1)],
            []);
        separationOrder.WaitForPurchase();
        return separationOrder;
    }

    private static async Task SeedStockForSeparationAsync(FakeStockRepository stockRepo, SeparationOrder separationOrder)
    {
        foreach (var part in separationOrder.Parts)
        {
            await stockRepo.AddAsync(GarageFlow.Domain.Stock.Stock.Create(part.PartId, StockItemType.Part, 100m, 0m));
        }

        foreach (var supply in separationOrder.Supplies)
        {
            await stockRepo.AddAsync(GarageFlow.Domain.Stock.Stock.Create(supply.SupplyId, StockItemType.Supply, 100m, 0m));
        }
    }

    private static async Task<(CompletePurchaseOrderHandler Handler, Guid PurchaseOrderId)> BuildStartedPurchaseOrderAndGetHandler(
        FakePurchaseOrderRepository purchaseRepo,
        FakeSeparationOrderRepository separationRepo,
        FakeStockRepository stockRepo,
        FakeEmployeeRepository employeeRepo,
        Guid stockistId,
        IReadOnlyList<Guid> separationIds)
    {
        var supplierRepo = new FakeSupplierRepository();
        var supplier = ValidSupplier();
        await supplierRepo.AddAsync(supplier);

        var createHandler = new CreatePurchaseOrderHandler(purchaseRepo);
        var dto = await createHandler.HandleAsync(CreateCommandForSeparations(separationIds));

        var assignHandler = new AssignPurchaseOrderSupplierHandler(purchaseRepo, supplierRepo, employeeRepo);
        await assignHandler.HandleAsync(new AssignPurchaseOrderSupplierCommand(dto.Id, supplier.Id, stockistId));

        var startHandler = new StartPurchaseOrderHandler(purchaseRepo);
        await startHandler.HandleAsync(new StartPurchaseOrderCommand(dto.Id));

        return (new CompletePurchaseOrderHandler(purchaseRepo, separationRepo, stockRepo), dto.Id);
    }

    // --- Fluxo feliz: concluir compra retoma separações em WaitingPurchase ---

    [Fact]
    public async Task Complete_WithSeparationInWaitingPurchase_ReturnsCompletedAndResumes()
    {
        var purchaseRepo = new FakePurchaseOrderRepository();
        var separationRepo = new FakeSeparationOrderRepository();
        var stockRepo = new FakeStockRepository();
        var employeeRepo = new FakeEmployeeRepository();
        var stockist = await SeedEmployeeAsync(employeeRepo);

        var separationOrder = SeparationOrderInWaitingPurchase();
        await separationRepo.AddAsync(separationOrder);
        await SeedStockForSeparationAsync(stockRepo, separationOrder);

        var (handler, purchaseOrderId) = await BuildStartedPurchaseOrderAndGetHandler(
            purchaseRepo, separationRepo, stockRepo, employeeRepo, stockist.Id, [separationOrder.Id]);

        var result = await handler.HandleAsync(new CompletePurchaseOrderCommand(purchaseOrderId));

        result.Status.Should().Be(PurchaseOrderStatus.Completed);
        result.CompletedAt.Should().NotBeNull();
        separationOrder.Status.Should().Be(SeparationOrderStatus.WaitingPickup);
    }

    [Fact]
    public async Task Complete_WithMultipleSeparationsInWaitingPurchase_ResumesAll()
    {
        var purchaseRepo = new FakePurchaseOrderRepository();
        var separationRepo = new FakeSeparationOrderRepository();
        var stockRepo = new FakeStockRepository();
        var employeeRepo = new FakeEmployeeRepository();
        var stockist = await SeedEmployeeAsync(employeeRepo);

        var sep1 = SeparationOrderInWaitingPurchase();
        var sep2 = SeparationOrderInWaitingPurchase();
        await separationRepo.AddAsync(sep1);
        await separationRepo.AddAsync(sep2);
        await SeedStockForSeparationAsync(stockRepo, sep1);
        await SeedStockForSeparationAsync(stockRepo, sep2);

        var (handler, purchaseOrderId) = await BuildStartedPurchaseOrderAndGetHandler(
            purchaseRepo, separationRepo, stockRepo, employeeRepo, stockist.Id, [sep1.Id, sep2.Id]);

        var result = await handler.HandleAsync(new CompletePurchaseOrderCommand(purchaseOrderId));

        result.Status.Should().Be(PurchaseOrderStatus.Completed);
        sep1.Status.Should().Be(SeparationOrderStatus.WaitingPickup);
        sep2.Status.Should().Be(SeparationOrderStatus.WaitingPickup);
    }

    // --- Separação vinculada inexistente ---

    [Fact]
    public async Task Complete_WhenLinkedSeparationNotFound_ThrowsEntityNotFoundException()
    {
        var purchaseRepo = new FakePurchaseOrderRepository();
        var separationRepo = new FakeSeparationOrderRepository();
        var stockRepo = new FakeStockRepository();
        var employeeRepo = new FakeEmployeeRepository();
        var stockist = await SeedEmployeeAsync(employeeRepo);
        var nonExistentSeparationId = Guid.NewGuid();

        var (handler, purchaseOrderId) = await BuildStartedPurchaseOrderAndGetHandler(
            purchaseRepo, separationRepo, stockRepo, employeeRepo, stockist.Id, [nonExistentSeparationId]);

        var act = async () => await handler.HandleAsync(new CompletePurchaseOrderCommand(purchaseOrderId));

        await act.Should().ThrowAsync<EntityNotFoundException>()
            .WithMessage($"*{nonExistentSeparationId}*");
    }

    // --- Separação em estado inválido para retomada ---

    [Fact]
    public async Task Complete_WhenSeparationNotInWaitingPurchase_ThrowsStatusTransitionException()
    {
        var purchaseRepo = new FakePurchaseOrderRepository();
        var separationRepo = new FakeSeparationOrderRepository();
        var stockRepo = new FakeStockRepository();
        var employeeRepo = new FakeEmployeeRepository();
        var stockist = await SeedEmployeeAsync(employeeRepo);

        var separationOrder = SeparationOrder.Create(
            Guid.NewGuid(),
            [SeparationPartItem.Create(Guid.NewGuid(), "Filtro de óleo", 1)],
            []);
        separationOrder.Reserve();
        await separationRepo.AddAsync(separationOrder);
        await SeedStockForSeparationAsync(stockRepo, separationOrder);

        var (handler, purchaseOrderId) = await BuildStartedPurchaseOrderAndGetHandler(
            purchaseRepo, separationRepo, stockRepo, employeeRepo, stockist.Id, [separationOrder.Id]);

        var act = async () => await handler.HandleAsync(new CompletePurchaseOrderCommand(purchaseOrderId));

        await act.Should().ThrowAsync<InvalidSeparationOrderStatusTransitionException>()
            .WithMessage("Separação não está Aguardando Compra");
    }

    // --- Purchase order não encontrada ---

    [Fact]
    public async Task Complete_WhenPurchaseOrderNotFound_ThrowsEntityNotFoundException()
    {
        var purchaseRepo = new FakePurchaseOrderRepository();
        var separationRepo = new FakeSeparationOrderRepository();
        var stockRepo = new FakeStockRepository();
        var handler = new CompletePurchaseOrderHandler(purchaseRepo, separationRepo, stockRepo);

        var act = async () => await handler.HandleAsync(new CompletePurchaseOrderCommand(Guid.NewGuid()));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    // --- Transição inválida da purchase order ---

    [Fact]
    public async Task Complete_WhenPurchaseOrderNotStarted_ThrowsStatusTransitionException()
    {
        var purchaseRepo = new FakePurchaseOrderRepository();
        var separationRepo = new FakeSeparationOrderRepository();
        var stockRepo = new FakeStockRepository();

        var createHandler = new CreatePurchaseOrderHandler(purchaseRepo);
        var dto = await createHandler.HandleAsync(
            CreateCommandForSeparations([Guid.NewGuid()]));

        var handler = new CompletePurchaseOrderHandler(purchaseRepo, separationRepo, stockRepo);
        var act = async () => await handler.HandleAsync(new CompletePurchaseOrderCommand(dto.Id));

        await act.Should().ThrowAsync<InvalidPurchaseOrderStatusTransitionException>()
            .WithMessage("Ordem de Compra não está Iniciada");
    }
}
