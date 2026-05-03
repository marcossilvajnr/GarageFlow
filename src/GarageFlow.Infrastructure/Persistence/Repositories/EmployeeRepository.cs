using GarageFlow.Domain.Employees;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GarageFlow.Infrastructure.Persistence.Repositories;

internal sealed class EmployeeRepository(GarageFlowDbContext dbContext) : IEmployeeRepository
{
    public async Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.Employees.FindAsync([id], cancellationToken);

    public async Task<(IReadOnlyList<Employee> Items, int TotalCount)> ListAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Employees.AsNoTracking();
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        var isDuplicate = employee.DocumentType == Domain.Customers.CustomerDocumentType.Cpf
            ? await dbContext.Employees.AnyAsync(e => e.Cpf!.Value == employee.Cpf!.Value, cancellationToken)
            : await dbContext.Employees.AnyAsync(e => e.Cnpj!.Value == employee.Cnpj!.Value, cancellationToken);

        if (isDuplicate)
        {
            var message = employee.DocumentType == Domain.Customers.CustomerDocumentType.Cpf
                ? DomainErrorMessages.DuplicateEmployeeCpf
                : DomainErrorMessages.DuplicateEmployeeCnpj;
            throw new DuplicateDocumentException(message);
        }

        await dbContext.Employees.AddAsync(employee, cancellationToken);
    }

    public void Update(Employee employee)
        => dbContext.Employees.Update(employee);

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
                ? DomainErrorMessages.DuplicateEmployeeCpf
                : DomainErrorMessages.DuplicateEmployeeCnpj;

            throw new DuplicateDocumentException(message);
        }
    }
}