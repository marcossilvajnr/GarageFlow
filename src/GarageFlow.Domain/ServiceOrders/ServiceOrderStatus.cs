namespace GarageFlow.Domain.ServiceOrders;

public enum ServiceOrderStatus
{
    Received,
    InDiagnostic,
    WaitingApproval,
    InExecution,
    Finished,
    Delivered
}
