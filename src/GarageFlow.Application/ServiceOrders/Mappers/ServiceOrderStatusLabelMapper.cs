using GarageFlow.Application.ServiceOrders.Enums;

namespace GarageFlow.Application.ServiceOrders.Mappers;

public static class ServiceOrderStatusLabelMapper
{
    public static string ToLabel(ServiceOrderStatus status) =>
        status switch
        {
            ServiceOrderStatus.Received => "Recebida",
            ServiceOrderStatus.InDiagnostic => "Diagnóstico",
            ServiceOrderStatus.WaitingApproval => "Aguardando Aprovação",
            ServiceOrderStatus.Approved => "Orçamento aprovado",
            ServiceOrderStatus.Rejected => "Orçamento recusado",
            ServiceOrderStatus.InExecution => "Execução",
            ServiceOrderStatus.Finished => "Finalizada",
            ServiceOrderStatus.Delivered => "Entregue",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
}
