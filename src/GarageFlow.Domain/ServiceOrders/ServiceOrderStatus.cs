namespace GarageFlow.Domain.ServiceOrders;

public enum ServiceOrderStatus
{
    Received = 0,
    InDiagnostic = 1,
    WaitingApproval = 2,
    InExecution = 3,
    Finished = 4,
    Delivered = 5,
    Approved = 6,
    Rejected = 7
}
