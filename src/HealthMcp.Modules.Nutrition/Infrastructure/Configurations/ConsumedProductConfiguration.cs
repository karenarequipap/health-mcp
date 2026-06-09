using HealthMcp.Modules.Nutrition.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthMcp.Modules.Nutrition.Infrastructure.Configurations;

public class ConsumedProductConfiguration : IEntityTypeConfiguration<ConsumedProduct>
{
    public void Configure(EntityTypeBuilder<ConsumedProduct> builder)
    {
        builder.ToTable("ConsumedProducts");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.QuantityGrams).IsRequired();

        builder.HasOne(c => c.Meal)
            .WithMany(m => m.ConsumedProducts)
            .HasForeignKey(c => c.MealId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Product)
            .WithMany()
            .HasForeignKey(c => c.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.MealId, c.ProductId, c.QuantityGrams }).IsUnique();
    }
}
