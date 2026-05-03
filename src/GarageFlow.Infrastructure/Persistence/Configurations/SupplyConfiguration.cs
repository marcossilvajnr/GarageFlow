using GarageFlow.Domain.Supplies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GarageFlow.Infrastructure.Persistence.Configurations;

internal sealed class SupplyConfiguration : IEntityTypeConfiguration<Supply>
{
    public void Configure(EntityTypeBuilder<Supply> builder)
    {
        builder.ToTable("supplies");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");

        builder.Property(s => s.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(s => s.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.HasIndex(s => s.Code).IsUnique().HasDatabaseName("ix_supplies_code");
        builder.Property(s => s.UnitOfMeasure).HasColumnName("unit_of_measure").HasMaxLength(20).IsRequired();

        builder.Property(s => s.BaseCost).HasColumnName("base_cost").HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(s => s.PreferredSupplierId).HasColumnName("preferred_supplier_id");
        builder.Property(s => s.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(s => s.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");
    }
}
