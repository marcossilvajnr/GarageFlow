using GarageFlow.Domain.Employees;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Tests.Application.Employees;

internal sealed class FakeEmployeeRepository : IEmployeeRepository
{
    private readonly List<Employee> _employees = [];

    public Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_employees.FirstOrDefault(e => e.Id == id));

    public Task<(IReadOnlyList<Employee> Items, int TotalCount)> ListAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var total = _employees.Count;
        var items = (IReadOnlyList<Employee>)_employees
            .Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult((items, total));
    }

    public Task AddAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        var isDuplicate = employee.DocumentType == GarageFlow.Domain.Customers.CustomerDocumentType.Cpf
            ? _employees.Any(e => e.Cpf?.Value == employee.Cpf!.Value)
            : _employees.Any(e => e.Cnpj?.Value == employee.Cnpj!.Value);

        if (isDuplicate)
        {
            var message = employee.DocumentType == GarageFlow.Domain.Customers.CustomerDocumentType.Cpf
                ? DomainErrorMessages.DuplicateEmployeeCpf
                : DomainErrorMessages.DuplicateEmployeeCnpj;
            throw new DuplicateDocumentException(message);
        }

        _employees.Add(employee);
        return Task.CompletedTask;
    }

    public void Update(Employee employee) { }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public IReadOnlyList<Employee> All => _employees;
}