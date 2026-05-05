using FluentAssertions;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Stock;
using GarageFlow.Domain.Supplies;

namespace GarageFlow.Tests.Domain.Stock;

public sealed class StockTests
{
    [Fact]
    public void Create_WithValidData_ShouldInitializeInvariant()
    {
        var stock = global::GarageFlow.Domain.Stock.Stock.Create(Guid.NewGuid(), StockItemType.Part, 10m, 2m);

        stock.TotalQuantity.Should().Be(10m);
        stock.AvailableQuantity.Should().Be(10m);
        stock.ReservedQuantity.Should().Be(0m);
    }

    [Fact]
    public void Reserve_WithSufficientAvailable_ShouldReserve()
    {
        var stock = global::GarageFlow.Domain.Stock.Stock.Create(Guid.NewGuid(), StockItemType.Part, 10m, 0m);

        stock.Reserve(3m);

        stock.TotalQuantity.Should().Be(10m);
        stock.ReservedQuantity.Should().Be(3m);
        stock.AvailableQuantity.Should().Be(7m);
    }

    [Fact]
    public void Reserve_WithInsufficientAvailable_ShouldThrowConflict()
    {
        var stock = global::GarageFlow.Domain.Stock.Stock.Create(Guid.NewGuid(), StockItemType.Part, 5m, 0m);

        var act = () => stock.Reserve(6m);

        act.Should().Throw<StockQuantityConflictException>();
    }

    [Fact]
    public void Consume_WithReserved_ShouldDecreaseTotalAndReserved()
    {
        var stock = global::GarageFlow.Domain.Stock.Stock.Create(Guid.NewGuid(), StockItemType.Part, 10m, 0m);
        stock.Reserve(4m);

        stock.Consume(2m);

        stock.TotalQuantity.Should().Be(8m);
        stock.ReservedQuantity.Should().Be(2m);
        stock.AvailableQuantity.Should().Be(6m);
    }

    [Fact]
    public void Release_WithSupply_ShouldReleaseReservedQuantity()
    {
        var stock = global::GarageFlow.Domain.Stock.Stock.Create(Guid.NewGuid(), StockItemType.Supply, 10m, 0m);
        stock.Reserve(2m);

        stock.Release(1m);

        stock.TotalQuantity.Should().Be(10m);
        stock.ReservedQuantity.Should().Be(1m);
        stock.AvailableQuantity.Should().Be(9m);
    }

    [Fact]
    public void Adjust_WithoutReason_ShouldThrowDomainException()
    {
        var stock = global::GarageFlow.Domain.Stock.Stock.Create(Guid.NewGuid(), StockItemType.Part, 10m, 0m);

        var act = () => stock.Adjust(2m, string.Empty);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Adjust_NegativeBeyondReserved_ShouldThrowConflict()
    {
        var stock = global::GarageFlow.Domain.Stock.Stock.Create(Guid.NewGuid(), StockItemType.Part, 10m, 0m);
        stock.Reserve(7m);

        var act = () => stock.Adjust(-4m, "Inventário");

        act.Should().Throw<StockQuantityConflictException>();
    }

    [Fact]
    public void Adjust_WithNegativeDelta_ShouldRecordNegativeOperationQuantity()
    {
        var stock = global::GarageFlow.Domain.Stock.Stock.Create(Guid.NewGuid(), StockItemType.Part, 10m, 0m);

        stock.Adjust(-2m, "Perda de inventário");

        stock.TotalQuantity.Should().Be(8m);
        stock.AvailableQuantity.Should().Be(8m);
        stock.Operations.Should().HaveCount(1);
        stock.Operations[0].Type.Should().Be(StockOperationType.Adjust);
        stock.Operations[0].Quantity.Should().Be(-2m);
    }
}
