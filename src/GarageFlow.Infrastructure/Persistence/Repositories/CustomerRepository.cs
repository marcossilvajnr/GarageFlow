using GarageFlow.Domain.Customers;
using GarageFlow.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GarageFlow.Infrastructure.Persistence.Repositories;

internal sealed class CustomerRepository(GarageFlowDbContext dbContext) : ICustomerRepository
{
    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.Customers.FindAsync([id], cancellationToken);

    public async Task<(IReadOnlyList<Customer> Items, int TotalCount)> ListAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Customers.AsNoTracking();
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        var isDuplicate = customer.DocumentType == CustomerDocumentType.Cpf
            ? await dbContext.Customers.AnyAsync(c => c.Cpf!.Value == customer.Cpf!.Value, cancellationToken)
            : await dbContext.Customers.AnyAsync(c => c.Cnpj!.Value == customer.Cnpj!.Value, cancellationToken);

        if (isDuplicate)
        {
            var message = customer.DocumentType == CustomerDocumentType.Cpf
                ? CustomersErrorMessages.DuplicateCpf
                : CustomersErrorMessages.DuplicateCnpj;
            throw new DuplicateDocumentException(message);
        }

        await dbContext.Customers.AddAsync(customer, cancellationToken);
    }

    public void Update(Customer customer)
        => dbContext.Customers.Update(customer);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException { SqlState: "23505" } pgEx)
        {
            var message = pgEx.ConstraintName?.Contains("cpf") == true
                ? CustomersErrorMessages.DuplicateCpf
                : CustomersErrorMessages.DuplicateCnpj;

            throw new DuplicateDocumentException(message);
        }
    }
}
