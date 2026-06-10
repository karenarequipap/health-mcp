using HealthMcp.Modules.Nutrition.Entities;
using HealthMcp.Modules.Nutrition.Infrastructure;
using HealthMcp.Modules.Nutrition.McpTools;
using Microsoft.EntityFrameworkCore;

namespace HealthMcp.Modules.Nutrition.Tests.McpTools;

public class GetDailyConsumptionByDateRangeToolTests
{
    private async Task<NutritionDbContext> SeedDatabase()
    {
        var options = new DbContextOptionsBuilder<NutritionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var db = new NutritionDbContext(options);

        var eggType = new MealType { Name = "Breakfast" };
        db.MealTypes.Add(eggType);
        await db.SaveChangesAsync();

        var eggs = new Product { Name = "Eggs", Calories = 155, Protein = 13, Fats = 11, Carbohydrates = 1.1m };
        db.Products.Add(eggs);
        await db.SaveChangesAsync();

        var meal = new Meal { Date = new DateOnly(2026, 2, 1), MealTypeId = eggType.Id };
        db.Meals.Add(meal);
        await db.SaveChangesAsync();

        db.ConsumedProducts.Add(new ConsumedProduct { MealId = meal.Id, ProductId = eggs.Id, QuantityGrams = 60 });
        await db.SaveChangesAsync();

        return db;
    }

    [Fact]
    public async Task ReturnsAggregatedDailyTotals()
    {
        await using var db = await SeedDatabase();

        var result = await GetDailyConsumptionByDateRangeTool.GetDailyConsumptionByDateRange(
            db, "2026-02-01", "2026-02-01", CancellationToken.None);

        var day = Assert.Single(result.days);
        Assert.Equal("2026-02-01", day.date);
        // 60g of eggs * (155 kcal / 100g) = 93 kcal
        Assert.Equal(93.00m, day.calories);
        // 60g * 13g protein / 100 = 7.8
        Assert.Equal(7.80m, day.protein);
    }
}
