using HealthMcp.Modules.Nutrition.Entities;
using HealthMcp.Modules.Nutrition.Infrastructure;
using HealthMcp.Modules.Nutrition.McpTools;
using Microsoft.EntityFrameworkCore;

namespace HealthMcp.Modules.Nutrition.Tests.McpTools;

public class GetNutrientConsumptionByDateRangeToolTests
{
    private async Task<NutritionDbContext> SeedDatabase()
    {
        var options = new DbContextOptionsBuilder<NutritionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var db = new NutritionDbContext(options);

        var mt = new MealType { Name = "Breakfast" };
        db.MealTypes.Add(mt);
        await db.SaveChangesAsync();

        var eggs = new Product { Name = "Eggs", Calories = 155, Protein = 13, Fats = 11, Carbohydrates = 1.1m, Saturated = 3.2m };
        var cheese = new Product { Name = "Feta cheese", Calories = 276, Protein = 16.5m, Fats = 23, Carbohydrates = 0.7m, Saturated = 16.1m };
        db.Products.AddRange(eggs, cheese);
        await db.SaveChangesAsync();

        var meal = new Meal { Date = new DateOnly(2026, 2, 1), MealTypeId = mt.Id };
        db.Meals.Add(meal);
        await db.SaveChangesAsync();

        db.ConsumedProducts.AddRange(
            new ConsumedProduct { MealId = meal.Id, ProductId = eggs.Id, QuantityGrams = 60 },
            new ConsumedProduct { MealId = meal.Id, ProductId = cheese.Id, QuantityGrams = 30 }
        );
        await db.SaveChangesAsync();

        return db;
    }

    [Fact]
    public async Task AggregatedMode_ReturnsTotalPerDay()
    {
        await using var db = await SeedDatabase();

        var result = await GetNutrientConsumptionByDateRangeTool.GetNutrientConsumptionByDateRange(
            db, "2026-02-01", "2026-02-01", "saturated", "aggregated", CancellationToken.None);

        var day = Assert.Single(result.Days);
        // 60g eggs * 3.2/100 + 30g cheese * 16.1/100 = 1.92 + 4.83 = 6.75
        Assert.Equal(6.75m, day.Value);
    }

    [Fact]
    public async Task DetailedMode_ReturnsBreakdownByProduct()
    {
        await using var db = await SeedDatabase();

        var result = await GetNutrientConsumptionByDateRangeTool.GetNutrientConsumptionByDateRange(
            db, "2026-02-01", "2026-02-01", "saturated", "detailed", CancellationToken.None);

        var day = Assert.Single(result.Days);
        Assert.Equal(2, day.Products!.Count);
        Assert.Contains(day.Products!, p => p.Name == "Eggs" && p.Value == 1.92m);
        Assert.Contains(day.Products!, p => p.Name == "Feta cheese" && p.Value == 4.83m);
    }

    [Fact]
    public async Task UnknownNutrient_ReturnsError()
    {
        await using var db = await SeedDatabase();

        var result = await GetNutrientConsumptionByDateRangeTool.GetNutrientConsumptionByDateRange(
            db, "2026-02-01", "2026-02-01", "unknown", "aggregated", CancellationToken.None);

        Assert.True(result.IsError);
    }
}
