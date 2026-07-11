using FluentAssertions;
using GarageFlow.Application.ServiceOrders.Enums;
using GarageFlow.Application.ServiceOrders.Mappers;

namespace GarageFlow.Tests.Application.ServiceOrders;

public sealed class ServiceOrderStatusLabelMapperTests
{
    [Theory]
    [InlineData(ServiceOrderStatus.Received, "Recebida")]
    [InlineData(ServiceOrderStatus.InDiagnostic, "Diagnóstico")]
    [InlineData(ServiceOrderStatus.WaitingApproval, "Aguardando Aprovação")]
    [InlineData(ServiceOrderStatus.Approved, "Orçamento aprovado")]
    [InlineData(ServiceOrderStatus.Rejected, "Orçamento recusado")]
    [InlineData(ServiceOrderStatus.InExecution, "Execução")]
    [InlineData(ServiceOrderStatus.Finished, "Finalizada")]
    [InlineData(ServiceOrderStatus.Delivered, "Entregue")]
    public void ToLabel_WithEachKnownStatus_ReturnsExpectedLabel(ServiceOrderStatus status, string expectedLabel)
    {
        ServiceOrderStatusLabelMapper.ToLabel(status).Should().Be(expectedLabel);
    }

    [Fact]
    public void ToLabel_CoversAllEnumValues()
    {
        foreach (var status in Enum.GetValues<ServiceOrderStatus>())
        {
            var act = () => ServiceOrderStatusLabelMapper.ToLabel(status);
            act.Should().NotThrow($"status {status} must have a mapped label");
        }
    }

    [Fact]
    public void ToLabel_WithUnknownStatus_ThrowsArgumentOutOfRangeException()
    {
        var unknownStatus = (ServiceOrderStatus)999;

        var act = () => ServiceOrderStatusLabelMapper.ToLabel(unknownStatus);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
