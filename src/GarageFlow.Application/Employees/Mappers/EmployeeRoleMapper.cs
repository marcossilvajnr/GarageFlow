using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;

using ApplicationEmployeeRole = GarageFlow.Application.Employees.Enums.EmployeeRole;
using DomainEmployeeRole = GarageFlow.Domain.Employees.EmployeeRole;

namespace GarageFlow.Application.Employees.Mappers;

internal static class EmployeeRoleMapper
{
    internal static DomainEmployeeRole ToDomain(ApplicationEmployeeRole role) =>
        role switch
        {
            ApplicationEmployeeRole.Attendant => DomainEmployeeRole.Attendant,
            ApplicationEmployeeRole.Mechanic => DomainEmployeeRole.Mechanic,
            ApplicationEmployeeRole.Stockist => DomainEmployeeRole.Stockist,
            ApplicationEmployeeRole.Administrative => DomainEmployeeRole.Administrative,
            _ => throw new DomainException(DomainErrorMessages.InvalidEmployeeRole)
        };

    internal static ApplicationEmployeeRole ToApplication(DomainEmployeeRole role) =>
        role switch
        {
            DomainEmployeeRole.Attendant => ApplicationEmployeeRole.Attendant,
            DomainEmployeeRole.Mechanic => ApplicationEmployeeRole.Mechanic,
            DomainEmployeeRole.Stockist => ApplicationEmployeeRole.Stockist,
            DomainEmployeeRole.Administrative => ApplicationEmployeeRole.Administrative,
            _ => throw new DomainException(DomainErrorMessages.InvalidEmployeeRole)
        };
}
