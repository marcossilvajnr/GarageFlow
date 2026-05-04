using GarageFlow.Domain.ServiceOrders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace GarageFlow.Infrastructure.Persistence.Configurations.ServiceOrders;

internal sealed class ServiceOrderConfiguration : IEntityTypeConfiguration<ServiceOrder>
{
    private static readonly ValueConverter<List<Guid>, string> GuidListConverter = new(
        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
        v => JsonSerializer.Deserialize<List<Guid>>(v, (JsonSerializerOptions?)null) ?? new List<Guid>());
    private static readonly ValueComparer<List<Guid>> GuidListComparer = new(
        (a, b) => a!.SequenceEqual(b!),
        list => list.Aggregate(0, (current, value) => HashCode.Combine(current, value.GetHashCode())),
        list => list.ToList());

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

        builder.OwnsOne(so => so.Diagnostic, diagBuilder =>
        {
            diagBuilder.ToTable("service_order_diagnostics");
            diagBuilder.WithOwner().HasForeignKey(d => d.ServiceOrderId);

            diagBuilder.HasKey(d => d.ServiceOrderId);
            diagBuilder.Property(d => d.Id).HasColumnName("id");
            diagBuilder.Property(d => d.ServiceOrderId).HasColumnName("service_order_id").IsRequired();
            diagBuilder.HasIndex(d => d.Id).IsUnique().HasDatabaseName("ix_service_order_diagnostics_id");
            diagBuilder.Property(d => d.MechanicId).HasColumnName("mechanic_id").IsRequired();
            diagBuilder.Property(d => d.Description).HasColumnName("description").HasMaxLength(2000);
            diagBuilder.Property(d => d.StartedAt).HasColumnName("started_at").IsRequired();
            diagBuilder.Property(d => d.CompletedAt).HasColumnName("completed_at");
            diagBuilder.Property(d => d.Status)
                .HasColumnName("status")
                .HasConversion<int>()
                .IsRequired();

            diagBuilder.Property(d => d.SelectedServiceIds)
                .HasColumnName("selected_service_ids")
                .HasConversion(GuidListConverter)
                .Metadata.SetValueComparer(GuidListComparer);
            diagBuilder.Property(d => d.SelectedServiceIds)
                .IsRequired()
                .HasDefaultValue(new List<Guid>());
        });

        builder.Navigation(so => so.Diagnostic).AutoInclude();

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

        builder.OwnsOne(so => so.Quote, quoteBuilder =>
        {
            quoteBuilder.ToTable("service_order_quotes");
            quoteBuilder.WithOwner().HasForeignKey(q => q.ServiceOrderId);

            quoteBuilder.HasKey(q => q.ServiceOrderId);
            quoteBuilder.Property(q => q.Id).HasColumnName("id").IsRequired();
            quoteBuilder.HasIndex(q => q.Id).IsUnique().HasDatabaseName("ix_service_order_quotes_id");
            quoteBuilder.Property(q => q.ServiceOrderId).HasColumnName("service_order_id").IsRequired();
            quoteBuilder.Property(q => q.TotalAmount).HasColumnName("total_amount").HasColumnType("numeric(18,2)").IsRequired();
            quoteBuilder.Property(q => q.Status).HasColumnName("status").HasConversion<int>().IsRequired();
            quoteBuilder.Property(q => q.GeneratedAt).HasColumnName("generated_at").IsRequired();
            quoteBuilder.Property(q => q.AcceptedAt).HasColumnName("accepted_at");
            quoteBuilder.Property(q => q.RejectedAt).HasColumnName("rejected_at");
            quoteBuilder.Property(q => q.RejectionReason).HasColumnName("rejection_reason").HasMaxLength(1000);

            quoteBuilder.OwnsMany(q => q.Items, itemBuilder =>
            {
                itemBuilder.ToTable("service_order_quote_items");
                itemBuilder.WithOwner().HasForeignKey("service_order_id");

                itemBuilder.Property(i => i.Id).HasColumnName("id").ValueGeneratedNever();
                itemBuilder.HasKey("service_order_id", nameof(QuoteItem.Id));
                itemBuilder.Property(i => i.ServiceId).HasColumnName("service_id").IsRequired();
                itemBuilder.Property(i => i.ServiceName).HasColumnName("service_name").HasMaxLength(200).IsRequired();
                itemBuilder.Property(i => i.LaborPrice).HasColumnName("labor_price").HasColumnType("numeric(18,2)").IsRequired();
                itemBuilder.Property(i => i.PartsTotal).HasColumnName("parts_total").HasColumnType("numeric(18,2)").IsRequired();
                itemBuilder.Property(i => i.SuppliesTotal).HasColumnName("supplies_total").HasColumnType("numeric(18,2)").IsRequired();
                itemBuilder.Property(i => i.Subtotal).HasColumnName("subtotal").HasColumnType("numeric(18,2)").IsRequired();
            });

            quoteBuilder.Navigation(q => q.Items).HasField("_items");
        });

        builder.Navigation(so => so.Quote).AutoInclude();
    }
}
