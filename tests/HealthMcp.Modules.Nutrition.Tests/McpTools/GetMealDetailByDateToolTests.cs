using HealthMcp.Modules.Nutrition.Entities;
using HealthMcp.Modules.Nutrition.Infrastructure;
using HealthMcp.Modules.Nutrition.McpTools;
using Microsoft.EntityFrameworkCore;

namespace HealthMcp.Modules.Nutrition.Tests.McpTools;

public class GetMealDetailByDateToolTests
{
    [Fact]
    public async Task ReturnsMealBreakdownForDate()
    {
        var options = new DbContextOptionsBuilder<NutritionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        await using var db = new NutritionDbContext(options);

        var breakfast = new MealType { Name = "Breakfast" };
        var lunch = new MealType { Name = "Lunch" };
        db.MealTypes.AddRange(breakfast, lunch);
        await db.SaveChangesAsync();

        var eggs = new Product { Name = "Eggs", Calories = 155, Protein = 13, Fats = 11, Carbohydrates = 1.1m };
        db.Products.Add(eggs);
        await db.SaveChangesAsync();

        var meal = new Meal { Date = new DateOnly(2026, 2, 1), MealTypeId = breakfast.Id };
        db.Meals.Add(meal);
        await db.SaveChangesAsync();

        db.ConsumedProducts.Add(new ConsumedProduct { MealId = meal.Id, ProductId = eggs.Id, QuantityGrams = 60 });
        await db.SaveChangesAsync();

        var result = await GetMealDetailByDateTool.GetMealDetailByDate(
            db, "2026-02-01", CancellationToken.None);

        var m = Assert.Single(result.meals);
        Assert.Equal("Breakfast", m.mealType);
        var p = Assert.Single(m.products);
        Assert.Equal("Eggs", p.name);
        Assert.Equal(60, p.quantityGrams);
        Assert.Equal(93.00m, p.calories); // 60 * 155 / 100
    }

    [Fact]
    public async Task ReturnsEmptyWhenNoDataForDate()
    {
        var options = new DbContextOptionsBuilder<NutritionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        await using var db = new NutritionDbContext(options);

        var result = await GetMealDetailByDateTool.GetMealDetailByDate(
            db, "2099-01-01", CancellationToken.None);

        Assert.Empty(result.meals);
    }
}
