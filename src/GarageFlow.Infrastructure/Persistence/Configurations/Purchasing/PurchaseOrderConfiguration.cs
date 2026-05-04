using GarageFlow.Domain.Purchasing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GarageFlow.Infrastructure.Persistence.Configurations.Purchasing;

internal sealed class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("purchase_orders");

        builder.HasKey(po => po.Id);
        builder.Property(po => po.Id).HasColumnName("id");

        builder.Property(po => po.SupplierId).HasColumnName("supplier_id");
        builder.Property(po => po.Status).HasColumnName("status").HasConversion<int>().IsRequired();
        builder.Property(po => po.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(po => po.StartedAt).HasColumnName("started_at");
        builder.Property(po => po.CompletedAt).HasColumnName("completed_at");

        builder.Ignore(po => po.SeparationOrderIds);

        builder.OwnsMany(po => po.SeparationOrderRefs, b =>
        {
            b.ToTable("purchase_order_separation_refs");
            b.WithOwner().HasForeignKey("purchase_order_id");

            b.HasKey("purchase_order_id", nameof(PurchaseOrderSeparationRef.SeparationOrderId));
            b.Property(r => r.SeparationOrderId).HasColumnName("separation_order_id").IsRequired();
        });

        builder.Navigation(po => po.SeparationOrderRefs).HasField("_separationOrderRefs");

        builder.OwnsMany(po => po.Items, b =>
        {
            b.ToTable("purchase_order_items");
            b.WithOwner().HasForeignKey("purchase_order_id");

            b.HasKey("purchase_order_id", nameof(PurchaseItem.ItemId));
            b.Property(i => i.ItemId).HasColumnName("item_id").IsRequired();
            b.Property(i => i.ItemType).HasColumnName("item_type").HasConversion<int>().IsRequired();
            b.Property(i => i.ItemName).HasColumnName("item_name").HasMaxLength(200).IsRequired();
            b.Property(i => i.Quantity).HasColumnName("quantity").HasColumnType("numeric(18,4)").IsRequired();
            b.Property(i => i.UnitPrice).HasColumnName("unit_price").HasColumnType("numeric(18,2)").IsRequired();
            b.Ignore(i => i.Subtotal);
        });

        builder.Navigation(po => po.Items).HasField("_items");

        builder.HasIndex(po => po.Status).HasDatabaseName("ix_purchase_orders_status");
        builder.HasIndex(po => po.SupplierId).HasDatabaseName("ix_purchase_orders_supplier_id");
    }
}
