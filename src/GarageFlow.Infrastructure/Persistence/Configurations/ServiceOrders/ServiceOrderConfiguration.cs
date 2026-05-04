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
    }
}
