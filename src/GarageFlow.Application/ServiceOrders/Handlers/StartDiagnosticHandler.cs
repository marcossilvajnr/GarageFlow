using GarageFlow.Application.ServiceOrders.Commands;
using GarageFlow.Application.ServiceOrders.DTOs;
using GarageFlow.Domain.Employees;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.ServiceOrders;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Application.ServiceOrders.Handlers;

public sealed class StartDiagnosticHandler(
    IServiceOrderRepository serviceOrderRepository,
    IEmployeeRepository employeeRepository)
{
    public async Task<ServiceOrderDto> HandleAsync(
        StartDiagnosticCommand command,
        CancellationToken cancellationToken = default)
    {
        await Employees.EmployeeActorValidator.ValidateAsync(
            employeeRepository,
            command.MechanicId,
            DomainErrorMessages.InvalidDiagnosticMechanicId,
            [EmployeeRole.Mechanic, EmployeeRole.Administrative],
            cancellationToken);

        var serviceOrder = await serviceOrderRepository.GetByIdAsync(command.ServiceOrderId, cancellationToken);
        if (serviceOrder is null)
            throw new EntityNotFoundException(DomainErrorMessages.ServiceOrderNotFound(command.ServiceOrderId));

        serviceOrder.StartDiagnostic(command.MechanicId);

        await serviceOrderRepository.SaveChangesAsync(cancellationToken);

        return ServiceOrderMapper.ToDto(serviceOrder);
    }
}
