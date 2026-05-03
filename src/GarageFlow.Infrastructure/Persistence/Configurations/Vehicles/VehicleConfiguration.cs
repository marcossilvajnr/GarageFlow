using GarageFlow.Domain.Vehicles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GarageFlow.Infrastructure.Persistence.Configurations.Vehicles;

internal sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("vehicles");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasColumnName("id");

        builder.Property(v => v.CustomerId).HasColumnName("customer_id").IsRequired();
        builder.Property(v => v.Make).HasColumnName("make").HasMaxLength(100).IsRequired();
        builder.Property(v => v.Model).HasColumnName("model").HasMaxLength(100).IsRequired();
        builder.Property(v => v.Year).HasColumnName("year").IsRequired();
        builder.Property(v => v.Color).HasColumnName("color").HasMaxLength(50).IsRequired();
        builder.Property(v => v.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(v => v.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(v => v.UpdatedAt).HasColumnName("updated_at");

        builder.OwnsOne(v => v.LicensePlate, licensePlate =>
        {
            licensePlate.Property(x => x.Value).HasColumnName("license_plate").HasMaxLength(7).IsRequired();
            licensePlate.HasIndex(x => x.Value).IsUnique();
        });

        builder.OwnsOne(v => v.Renavam, renavam =>
        {
            renavam.Property(x => x.Value).HasColumnName("renavam").HasMaxLength(11).IsRequired();
            renavam.HasIndex(x => x.Value).IsUnique();
        });

        builder.HasIndex(v => v.CustomerId);
        builder.HasIndex(v => new { v.CustomerId, v.IsActive });
    }
}
