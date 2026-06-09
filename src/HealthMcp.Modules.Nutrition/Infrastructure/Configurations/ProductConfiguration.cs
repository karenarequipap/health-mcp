using HealthMcp.Modules.Nutrition.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthMcp.Modules.Nutrition.Infrastructure.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(500);
        builder.Property(p => p.Calories).IsRequired();
        builder.Property(p => p.Protein).IsRequired();
        builder.Property(p => p.Fats).IsRequired();
        builder.Property(p => p.Carbohydrates).IsRequired();

        builder.HasIndex(p => new { p.Name, p.Calories }).IsUnique();
    }
}
