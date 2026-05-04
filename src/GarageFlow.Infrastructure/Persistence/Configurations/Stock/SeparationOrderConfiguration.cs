using GarageFlow.Domain.Stock;
using GarageFlow.Domain.Supplies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GarageFlow.Infrastructure.Persistence.Configurations.Stock;

internal sealed class SeparationOrderConfiguration : IEntityTypeConfiguration<SeparationOrder>
{
    public void Configure(EntityTypeBuilder<SeparationOrder> builder)
    {
        builder.ToTable("separation_orders");

        builder.HasKey(so => so.Id);
        builder.Property(so => so.Id).HasColumnName("id");

        builder.Property(so => so.ExecutionOrderId).HasColumnName("execution_order_id").IsRequired();
        builder.Property(so => so.Status).HasColumnName("status").HasConversion<int>().IsRequired();
        builder.Property(so => so.StockistId).HasColumnName("stockist_id");
        builder.Property(so => so.ConfirmedByStockistAt).HasColumnName("confirmed_by_stockist_at");
        builder.Property(so => so.ConfirmedByMechanicAt).HasColumnName("confirmed_by_mechanic_at");
        builder.Property(so => so.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(so => so.ExecutionOrderId).HasDatabaseName("ix_separation_orders_execution_order_id");
        builder.HasIndex(so => so.Status).HasDatabaseName("ix_separation_orders_status");

        builder.OwnsMany(so => so.Parts, b =>
        {
            b.ToTable("separation_order_parts");
            b.WithOwner().HasForeignKey("separation_order_id");

            b.HasKey("separation_order_id", nameof(SeparationPartItem.PartId));
            b.Property(p => p.PartId).HasColumnName("part_id").IsRequired();
            b.Property(p => p.PartName).HasColumnName("part_name").HasMaxLength(200).IsRequired();
            b.Property(p => p.Quantity).HasColumnName("quantity").IsRequired();
            b.Property(p => p.IsReserved).HasColumnName("is_reserved").IsRequired();
        });

        builder.Navigation(so => so.Parts).HasField("_parts");

        builder.OwnsMany(so => so.Supplies, b =>
        {
            b.ToTable("separation_order_supplies");
            b.WithOwner().HasForeignKey("separation_order_id");

            b.HasKey("separation_order_id", nameof(SeparationSupplyItem.SupplyId));
            b.Property(s => s.SupplyId).HasColumnName("supply_id").IsRequired();
            b.Property(s => s.SupplyName).HasColumnName("supply_name").HasMaxLength(200).IsRequired();
            b.Property(s => s.Quantity).HasColumnName("quantity").HasColumnType("numeric(18,4)").IsRequired();
            b.Property(s => s.Unit).HasColumnName("unit").HasConversion<int>().IsRequired();
            b.Property(s => s.IsReserved).HasColumnName("is_reserved").IsRequired();
        });

        builder.Navigation(so => so.Supplies).HasField("_supplies");
    }
}
