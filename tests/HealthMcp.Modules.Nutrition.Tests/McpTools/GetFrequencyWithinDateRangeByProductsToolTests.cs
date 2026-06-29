using HealthMcp.Modules.Nutrition.Entities;
using HealthMcp.Modules.Nutrition.Infrastructure;
using HealthMcp.Modules.Nutrition.McpTools;
using Microsoft.EntityFrameworkCore;

namespace HealthMcp.Modules.Nutrition.Tests.McpTools;

public class GetFrequencyWithinDateRangeByProductsToolTests
{
    [Fact]
    public async Task ReturnsProductFrequencies()
    {
        var options = new DbContextOptionsBuilder<NutritionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        await using var db = new NutritionDbContext(options);

        var mt = new MealType { Name = "Breakfast" };
        db.MealTypes.Add(mt);
        await db.SaveChangesAsync();

        var eggs = new Product { Name = "Eggs", Calories = 155, Protein = 13, Fats = 11, Carbohydrates = 1.1m };
        var cheese = new Product { Name = "Feta cheese", Calories = 276, Protein = 16.5m, Fats = 23, Carbohydrates = 0.7m };
        db.Products.AddRange(eggs, cheese);
        await db.SaveChangesAsync();

        var meal1 = new Meal { Date = new DateOnly(2026, 2, 1), MealTypeId = mt.Id };
        var meal2 = new Meal { Date = new DateOnly(2026, 2, 2), MealTypeId = mt.Id };
        db.Meals.AddRange(meal1, meal2);
        await db.SaveChangesAsync();

        db.ConsumedProducts.AddRange(
            new ConsumedProduct { MealId = meal1.Id, ProductId = eggs.Id, QuantityGrams = 60 },
            new ConsumedProduct { MealId = meal2.Id, ProductId = eggs.Id, QuantityGrams = 60 },
            new ConsumedProduct { MealId = meal1.Id, ProductId = cheese.Id, QuantityGrams = 30 }
        );
        await db.SaveChangesAsync();

        var result = await GetFrequencyWithinDateRangeByProductsTool.GetFrequencyWithinDateRangeByProducts(
            db, "2026-02-01", "2026-02-02", CancellationToken.None);

        Assert.Equal(2, result.products.Count);
        var eggsFreq = result.products.First(p => p.name == "Eggs");
        Assert.Equal(2, eggsFreq.count);
        var cheeseFreq = result.products.First(p => p.name == "Feta cheese");
        Assert.Equal(1, cheeseFreq.count);
    }
}
