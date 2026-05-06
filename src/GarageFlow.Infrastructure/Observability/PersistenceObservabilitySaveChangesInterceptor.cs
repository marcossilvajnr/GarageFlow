using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace GarageFlow.Infrastructure.Observability;

public sealed class PersistenceObservabilitySaveChangesInterceptor(
    ILogger<PersistenceObservabilitySaveChangesInterceptor> logger)
    : SaveChangesInterceptor
{
    private static readonly HashSet<string> ObservableAggregates =
    [
        "Customer",
        "Vehicle",
        "Supplier",
        "Employee",
        "Service",
        "Part",
        "Supply",
        "ServiceOrder",
        "SeparationOrder",
        "Stock",
        "ExecutionOrder",
        "PurchaseOrder"
    ];

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        LogChanges(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        LogChanges(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void LogChanges(DbContext? context)
    {
        if (context is null)
            return;

        var correlationId = Activity.Current?.Id;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            var aggregate = entry.Entity.GetType().Name;
            if (!ObservableAggregates.Contains(aggregate))
                continue;

            var aggregateId = ResolveAggregateId(entry.Entity);

            if (entry.State == EntityState.Added)
            {
                logger.LogInformation(
                    "entity_created aggregate={Aggregate} aggregateId={AggregateId} action={Action} correlationId={CorrelationId}",
                    aggregate,
                    aggregateId,
                    "Created",
                    correlationId);
                continue;
            }

            if (entry.State != EntityState.Modified)
                continue;

            var stateTransitions = GetStateTransitions(entry).ToList();
            if (stateTransitions.Count > 0)
            {
                foreach (var transition in stateTransitions)
                {
                    logger.LogInformation(
                        "state_transition aggregate={Aggregate} aggregateId={AggregateId} transition={Transition} oldState={OldState} newState={NewState} correlationId={CorrelationId}",
                        aggregate,
                        aggregateId,
                        transition.PropertyName,
                        transition.OldState,
                        transition.NewState,
                        correlationId);
                }

                continue;
            }

            logger.LogInformation(
                "entity_updated aggregate={Aggregate} aggregateId={AggregateId} action={Action} correlationId={CorrelationId}",
                aggregate,
                aggregateId,
                "Updated",
                correlationId);
        }
    }

    private static string ResolveAggregateId(object entity)
    {
        var property = entity.GetType().GetProperty("Id");
        var value = property?.GetValue(entity);
        return value?.ToString() ?? "n/a";
    }

    private static IEnumerable<StateTransitionLog> GetStateTransitions(EntityEntry entry)
    {
        foreach (var property in entry.Properties)
        {
            if (!property.IsModified)
                continue;

            if (!property.Metadata.Name.EndsWith("Status", StringComparison.Ordinal))
                continue;

            var oldState = property.OriginalValue?.ToString();
            var newState = property.CurrentValue?.ToString();
            if (string.Equals(oldState, newState, StringComparison.Ordinal))
                continue;

            yield return new StateTransitionLog(property.Metadata.Name, oldState, newState);
        }
    }

    private sealed record StateTransitionLog(string PropertyName, string? OldState, string? NewState);
}
