using GarageFlow.Domain.ServiceOrders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GarageFlow.Infrastructure.Persistence.Configurations.ServiceOrders;

internal sealed class ServiceOrderConfiguration : IEntityTypeConfiguration<ServiceOrder>
{
    public void Configure(EntityTypeBuilder<ServiceOrder> builder)
    {
        builder.ToTable("service_orders");

        builder.HasKey(so => so.Id);
        builder.Property(so => so.Id).HasColumnName("id");

        builder.Property(so => so.CustomerId).HasColumnName("customer_id").IsRequired();
        builder.Property(so => so.VehicleId).HasColumnName("vehicle_id").IsRequired();
        builder.Property(so => so.Status).HasColumnName("status").IsRequired();
        builder.Property(so => so.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(so => so.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(so => so.CustomerId).HasDatabaseName("ix_service_orders_customer_id");
        builder.HasIndex(so => so.VehicleId).HasDatabaseName("ix_service_orders_vehicle_id");
        builder.HasIndex(so => so.Status).HasDatabaseName("ix_service_orders_status");

        builder.OwnsMany(so => so.Services, b =>
        {
            b.ToTable("service_order_services");
            b.WithOwner().HasForeignKey("service_order_id");

            b.Property(i => i.Id).HasColumnName("id").ValueGeneratedNever();
            b.HasKey("service_order_id", nameof(ServiceOrderServiceItem.Id));
            b.Property(i => i.ServiceId).HasColumnName("service_id").IsRequired();
            b.Property(i => i.Source).HasColumnName("source").HasConversion<int>().IsRequired();
            b.Property(i => i.AddedByActorId).HasColumnName("added_by_actor_id").IsRequired();
            b.Property(i => i.AddedAt).HasColumnName("added_at").IsRequired();
            b.Property(i => i.IsActive).HasColumnName("is_active").IsRequired();
            b.Property(i => i.RemovedAt).HasColumnName("removed_at");
            b.Property(i => i.RemovedByActorId).HasColumnName("removed_by_actor_id");
            b.Property(i => i.RemovalReason).HasColumnName("removal_reason").HasMaxLength(500);

            b.HasIndex("service_order_id", nameof(ServiceOrderServiceItem.ServiceId))
                .IsUnique()
                .HasFilter("\"is_active\" = true")
                .HasDatabaseName("ux_service_order_services_active");
        });

        builder.Navigation(so => so.Services).HasField("_services");

        builder.OwnsMany(so => so.ServiceHistory, b =>
        {
            b.ToTable("service_order_service_history");
            b.WithOwner().HasForeignKey("service_order_id");

            b.Property(h => h.Id).HasColumnName("id").ValueGeneratedNever();
            b.HasKey("service_order_id", nameof(ServiceOrderServiceHistory.Id));
            b.Property(h => h.ServiceId).HasColumnName("service_id").IsRequired();
            b.Property(h => h.Action).HasColumnName("action").HasConversion<int>().IsRequired();
            b.Property(h => h.Source).HasColumnName("source").HasConversion<int>().IsRequired();
            b.Property(h => h.ActorId).HasColumnName("actor_id").IsRequired();
            b.Property(h => h.OccurredAt).HasColumnName("occurred_at").IsRequired();
            b.Property(h => h.Reason).HasColumnName("reason").HasMaxLength(500);
        });

        builder.Navigation(so => so.ServiceHistory).HasField("_serviceHistory");
    }
}
