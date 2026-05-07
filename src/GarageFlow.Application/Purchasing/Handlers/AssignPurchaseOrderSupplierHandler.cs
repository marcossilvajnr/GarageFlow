using GarageFlow.Application.Purchasing.Commands;
using GarageFlow.Application.Purchasing.DTOs;
using GarageFlow.Domain.Employees;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Purchasing;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.Suppliers;

namespace GarageFlow.Application.Purchasing.Handlers;

public sealed class AssignPurchaseOrderSupplierHandler(
    IPurchaseOrderRepository purchaseOrderRepository,
    ISupplierRepository supplierRepository,
    IEmployeeRepository employeeRepository)
{
    public async Task<PurchaseOrderDto> HandleAsync(
        AssignPurchaseOrderSupplierCommand command,
        CancellationToken cancellationToken = default)
    {
        var employeeId = command.EmployeeId;
        await Employees.EmployeeActorValidator.ValidateAsync(
            employeeRepository,
            employeeId,
            DomainErrorMessages.InvalidPurchaseOrderActorEmployeeId,
            [EmployeeRole.Stockist, EmployeeRole.Administrative],
            cancellationToken);

        var purchaseOrder = await purchaseOrderRepository.GetByIdAsync(command.Id, cancellationToken);
        if (purchaseOrder is null)
            throw new EntityNotFoundException(DomainErrorMessages.PurchaseOrderNotFound(command.Id));

        var supplier = await supplierRepository.GetByIdAsync(command.SupplierId, cancellationToken);
        if (supplier is null)
            throw new DomainException(DomainErrorMessages.SupplierNotFound(command.SupplierId));

        purchaseOrder.AssignSupplier(command.SupplierId, employeeId);

        await purchaseOrderRepository.SaveChangesAsync(cancellationToken);

        return PurchaseOrderMapper.ToDto(purchaseOrder);
    }
}
