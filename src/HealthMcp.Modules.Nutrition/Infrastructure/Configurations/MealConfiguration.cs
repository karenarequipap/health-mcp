using HealthMcp.Modules.Nutrition.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthMcp.Modules.Nutrition.Infrastructure.Configurations;

public class MealConfiguration : IEntityTypeConfiguration<Meal>
{
    public void Configure(EntityTypeBuilder<Meal> builder)
    {
        builder.ToTable("Meals");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Date).IsRequired();

        builder.HasOne(m => m.MealType)
            .WithMany()
            .HasForeignKey(m => m.MealTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => new { m.Date, m.MealTypeId }).IsUnique();
    }
}
