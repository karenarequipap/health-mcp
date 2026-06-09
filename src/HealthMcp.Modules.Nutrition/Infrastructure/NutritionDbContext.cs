using HealthMcp.Modules.Nutrition.Entities;
using HealthMcp.Modules.Nutrition.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace HealthMcp.Modules.Nutrition.Infrastructure;

public class NutritionDbContext : DbContext
{
    public NutritionDbContext(DbContextOptions<NutritionDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<MealType> MealTypes => Set<MealType>();
    public DbSet<Meal> Meals => Set<Meal>();
    public DbSet<ConsumedProduct> ConsumedProducts => Set<ConsumedProduct>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new MealTypeConfiguration());
        modelBuilder.ApplyConfiguration(new MealConfiguration());
        modelBuilder.ApplyConfiguration(new ConsumedProductConfiguration());
    }
}
