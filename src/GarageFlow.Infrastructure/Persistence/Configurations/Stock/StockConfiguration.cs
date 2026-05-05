using DomainStock = GarageFlow.Domain.Stock.Stock;
using DomainStockOperation = GarageFlow.Domain.Stock.StockOperation;
using GarageFlow.Domain.Stock;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GarageFlow.Infrastructure.Persistence.Configurations.Stock;

internal sealed class StockConfiguration : IEntityTypeConfiguration<DomainStock>
{
    public void Configure(EntityTypeBuilder<DomainStock> builder)
    {
        builder.ToTable("stocks");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");

        builder.Property(s => s.ItemId).HasColumnName("item_id").IsRequired();
        builder.Property(s => s.ItemType).HasColumnName("item_type").HasConversion<int>().IsRequired();
        builder.Property(s => s.TotalQuantity).HasColumnName("total_quantity").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(s => s.AvailableQuantity).HasColumnName("available_quantity").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(s => s.ReservedQuantity).HasColumnName("reserved_quantity").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(s => s.MinimumQuantity).HasColumnName("minimum_quantity").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(s => s.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(s => new { s.ItemType, s.ItemId })
            .IsUnique()
            .HasDatabaseName("ux_stocks_item_type_item_id");

        builder.OwnsMany(s => s.Operations, b =>
        {
            b.ToTable("stock_operations");
            b.WithOwner().HasForeignKey("stock_id");

            b.HasKey(so => so.Id);
            b.Property(so => so.Id).HasColumnName("id").ValueGeneratedOnAdd();
            b.Property(so => so.Type).HasColumnName("type").HasConversion<int>().IsRequired();
            b.Property(so => so.Quantity).HasColumnName("quantity").HasColumnType("numeric(18,4)").IsRequired();
            b.Property(so => so.Reason).HasColumnName("reason").HasMaxLength(StockConstants.MaxReasonLength);
            b.Property(so => so.ReferenceId).HasColumnName("reference_id");
            b.Property(so => so.CreatedAt).HasColumnName("created_at").IsRequired();

            b.HasIndex("stock_id", nameof(DomainStockOperation.CreatedAt))
                .HasDatabaseName("ix_stock_operations_stock_id_created_at");
        });

        builder.Navigation(s => s.Operations).HasField("_operations");
    }
}
