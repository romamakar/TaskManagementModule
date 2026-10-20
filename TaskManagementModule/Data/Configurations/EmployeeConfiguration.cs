using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementModule.Data.Domain;

namespace TaskManagementModule.Data.Configurations;

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("employees");
        builder.HasKey(employee => employee.Id);

        builder.Property(employee => employee.FirstName)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(employee => employee.LastName)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(employee => employee.Email)
            .HasMaxLength(254)
            .IsRequired();

        builder.HasIndex(employee => employee.Email)
            .IsUnique();
        builder.HasIndex(employee => employee.IsActive);

        builder.HasData(
            new Employee { Id = 1L, FirstName = "Olena", LastName = "Koval", Email = "olena.koval@example.com", IsActive = true },
            new Employee { Id = 2L, FirstName = "Andrii", LastName = "Shevchenko", Email = "andrii.shevchenko@example.com", IsActive = true },
            new Employee { Id = 3L, FirstName = "Iryna", LastName = "Melnyk", Email = "iryna.melnyk@example.com", IsActive = false });
    }
}
