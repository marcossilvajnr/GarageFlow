using GarageFlow.Domain.Services;
using GarageFlow.Domain.Supplies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GarageFlow.Infrastructure.Persistence.Configurations.Services;

internal sealed class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("services");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");

        builder.Property(s => s.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.HasIndex(s => s.Code).IsUnique().HasDatabaseName("ix_services_code");

        builder.Property(s => s.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.HasIndex(s => s.Name).IsUnique().HasDatabaseName("ix_services_name");

        builder.Property(s => s.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(s => s.BasePrice).HasColumnName("base_price").HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(s => s.EstimatedDurationMinutes).HasColumnName("estimated_duration_minutes");
        builder.Property(s => s.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(s => s.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");

        builder.OwnsMany(s => s.Parts, partsBuilder =>
        {
            partsBuilder.ToTable("service_parts");
            partsBuilder.WithOwner().HasForeignKey("service_id");

            partsBuilder.HasKey("service_id", nameof(ServicePartItem.PartId));
            partsBuilder.Property(p => p.PartId).HasColumnName("part_id").IsRequired();
            partsBuilder.Property(p => p.PartName).HasColumnName("part_name").HasMaxLength(200).IsRequired();
            partsBuilder.Property(p => p.Quantity).HasColumnName("quantity").IsRequired();

            partsBuilder.HasIndex("service_id", nameof(ServicePartItem.PartId))
                .IsUnique()
                .HasDatabaseName("ix_service_parts_service_id_part_id");
        });

        builder.Navigation(s => s.Parts)
            .HasField("_parts");

        builder.OwnsMany(s => s.Supplies, suppliesBuilder =>
        {
            suppliesBuilder.ToTable("service_supplies");
            suppliesBuilder.WithOwner().HasForeignKey("service_id");

            suppliesBuilder.HasKey("service_id", nameof(ServiceSupplyItem.SupplyId));
            suppliesBuilder.Property(s => s.SupplyId).HasColumnName("supply_id").IsRequired();
            suppliesBuilder.Property(s => s.SupplyName).HasColumnName("supply_name").HasMaxLength(200).IsRequired();
            suppliesBuilder.Property(s => s.Quantity).HasColumnName("quantity").HasColumnType("numeric(18,4)").IsRequired();
            suppliesBuilder.Property(s => s.Unit).HasColumnName("unit").HasConversion<int>().IsRequired();

            suppliesBuilder.HasIndex("service_id", nameof(ServiceSupplyItem.SupplyId))
                .IsUnique()
                .HasDatabaseName("ix_service_supplies_service_id_supply_id");
        });

        builder.Navigation(s => s.Supplies)
            .HasField("_supplies");
    }
}
