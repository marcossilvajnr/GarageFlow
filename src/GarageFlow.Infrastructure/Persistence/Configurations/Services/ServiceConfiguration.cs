using GarageFlow.Domain.Services;
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
    }
}
