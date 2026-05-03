using GarageFlow.Domain.Employees;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GarageFlow.Infrastructure.Persistence.Configurations.Employees;

internal sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("employees");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(e => e.DocumentType).HasColumnName("document_type").IsRequired();
        builder.Property(e => e.Role).HasColumnName("role").IsRequired();
        builder.Property(e => e.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");

        builder.OwnsOne(e => e.Cpf, cpf =>
        {
            cpf.Property(x => x.Value).HasColumnName("cpf").HasMaxLength(11);
            cpf.HasIndex(x => x.Value).IsUnique().HasFilter("cpf IS NOT NULL");
        });

        builder.OwnsOne(e => e.Cnpj, cnpj =>
        {
            cnpj.Property(x => x.Value).HasColumnName("cnpj").HasMaxLength(14);
            cnpj.HasIndex(x => x.Value).IsUnique().HasFilter("cnpj IS NOT NULL");
        });

        builder.OwnsOne(e => e.Email, email =>
        {
            email.Property(x => x.Value).HasColumnName("email").HasMaxLength(320).IsRequired();
        });

        builder.OwnsOne(e => e.PhoneNumber, phone =>
        {
            phone.Property(x => x.Value).HasColumnName("phone_number").HasMaxLength(11).IsRequired();
        });

        builder.OwnsOne(e => e.Address, address =>
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
