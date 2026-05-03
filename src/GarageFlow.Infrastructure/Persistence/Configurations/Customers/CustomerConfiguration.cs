using GarageFlow.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GarageFlow.Infrastructure.Persistence.Configurations.Customers;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        builder.Property(c => c.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(c => c.DocumentType).HasColumnName("document_type").IsRequired();
        builder.Property(c => c.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at");

        builder.OwnsOne(c => c.Cpf, cpf =>
        {
            cpf.Property(x => x.Value).HasColumnName("cpf").HasMaxLength(11);
            cpf.HasIndex(x => x.Value).IsUnique().HasFilter("cpf IS NOT NULL");
        });

        builder.OwnsOne(c => c.Cnpj, cnpj =>
        {
            cnpj.Property(x => x.Value).HasColumnName("cnpj").HasMaxLength(14);
            cnpj.HasIndex(x => x.Value).IsUnique().HasFilter("cnpj IS NOT NULL");
        });

        builder.OwnsOne(c => c.Email, email =>
        {
            email.Property(x => x.Value).HasColumnName("email").HasMaxLength(320).IsRequired();
        });

        builder.OwnsOne(c => c.PhoneNumber, phone =>
        {
            phone.Property(x => x.Value).HasColumnName("phone_number").HasMaxLength(11).IsRequired();
        });

        builder.OwnsOne(c => c.Address, address =>
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
