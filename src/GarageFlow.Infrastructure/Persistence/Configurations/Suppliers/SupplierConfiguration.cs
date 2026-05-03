using GarageFlow.Domain.Suppliers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GarageFlow.Infrastructure.Persistence.Configurations.Suppliers;

internal sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("suppliers");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");

        builder.Property(s => s.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(s => s.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(s => s.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");

        builder.OwnsOne(s => s.Cnpj, cnpj =>
        {
            cnpj.Property(x => x.Value).HasColumnName("cnpj").HasMaxLength(14).IsRequired();
            cnpj.HasIndex(x => x.Value).IsUnique();
        });

        builder.OwnsOne(s => s.Email, email =>
        {
            email.Property(x => x.Value).HasColumnName("email").HasMaxLength(320).IsRequired();
        });

        builder.OwnsOne(s => s.PhoneNumber, phone =>
        {
            phone.Property(x => x.Value).HasColumnName("phone_number").HasMaxLength(11).IsRequired();
        });

        builder.OwnsOne(s => s.Address, address =>
        {
            address.Property(x => x.Street).HasColumnName("address_street").HasMaxLength(200).IsRequired();
            address.Property(x => x.Number).HasColumnName("address_number").HasMaxLength(10).IsRequired();
            address.Property(x => x.Complement).HasColumnName("address_complement").HasMaxLength(100);
            address.Property(x => x.Neighborhood).HasColumnName("address_neighborhood").HasMaxLength(100).IsRequired();
            address.Property(x => x.City).HasColumnName("address_city").HasMaxLength(100).IsRequired();
            address.Property(x => x.State).HasColumnName("address_state").HasMaxLength(2).IsRequired();
            address.Property(x => x.ZipCode).HasColumnName("address_zip_code").HasMaxLength(8).IsRequired();
        });
    }
}
