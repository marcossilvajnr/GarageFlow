using GarageFlow.Application.Suppliers.DTOs;
using GarageFlow.Application.Suppliers.Queries;
using GarageFlow.Domain.Suppliers;

namespace GarageFlow.Application.Suppliers.Handlers;

public sealed class GetSupplierByIdHandler(ISupplierRepository repository)
{
    public async Task<SupplierDto?> HandleAsync(GetSupplierByIdQuery query, CancellationToken cancellationToken = default)
    {
        var supplier = await repository.GetByIdAsync(query.Id, cancellationToken);
        return supplier is null ? null : SupplierMapper.ToDto(supplier);
    }
}
