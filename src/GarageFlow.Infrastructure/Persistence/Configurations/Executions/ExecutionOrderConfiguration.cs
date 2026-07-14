using GarageFlow.Domain.Executions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GarageFlow.Infrastructure.Persistence.Configurations.Executions;

internal sealed class ExecutionOrderConfiguration : IEntityTypeConfiguration<ExecutionOrder>
{
    public void Configure(EntityTypeBuilder<ExecutionOrder> builder)
    {
        builder.ToTable("execution_orders");

        builder.HasKey(eo => eo.Id);
        builder.Property(eo => eo.Id).HasColumnName("id");

        builder.Property(eo => eo.ServiceOrderId).HasColumnName("service_order_id").IsRequired();
        builder.Property(eo => eo.ServiceId).HasColumnName("service_id").IsRequired();
        builder.Property(eo => eo.MechanicId).HasColumnName("mechanic_id");
        builder.Property(eo => eo.Status).HasColumnName("status").HasConversion<int>().IsRequired();
        builder.Property(eo => eo.StartedAt).HasColumnName("started_at");
        builder.Property(eo => eo.CompletedAt).HasColumnName("completed_at");
        builder.Property(eo => eo.ActualTimeMinutes).HasColumnName("actual_time_minutes").HasColumnType("numeric(18,4)");
        builder.Property(eo => eo.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(eo => eo.ServiceOrderId).HasDatabaseName("ix_execution_orders_service_order_id");
        builder.HasIndex(eo => new { eo.ServiceOrderId, eo.ServiceId })
            .IsUnique()
            .HasDatabaseName("ux_execution_orders_service_order_service");
        builder.HasIndex(eo => eo.Status).HasDatabaseName("ix_execution_orders_status");
    }
}
