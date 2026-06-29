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

        Assert.False(result.IsError);
        Assert.Null(result.ErrorMessage);
        var m = Assert.Single(result.Meals);
        Assert.Equal("Breakfast", m.MealType);
        var p = Assert.Single(m.Products);
        Assert.Equal("Eggs", p.Name);
        Assert.Equal(60, p.QuantityGrams);
        Assert.Equal(93.00m, p.Calories); // 60 * 155 / 100
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

        Assert.False(result.IsError);
        Assert.Null(result.ErrorMessage);
        Assert.Empty(result.Meals);
    }

    [Fact]
    public async Task ReturnsErrorForInvalidDate()
    {
        var options = new DbContextOptionsBuilder<NutritionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        await using var db = new NutritionDbContext(options);

        var result = await GetMealDetailByDateTool.GetMealDetailByDate(
            db, "not-a-date", CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Contains("Invalid date format", result.ErrorMessage);
        Assert.Contains("not-a-date", result.ErrorMessage);
    }

    [Fact]
    public async Task ReturnsErrorForOutOfRangeDate()
    {
        var options = new DbContextOptionsBuilder<NutritionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        await using var db = new NutritionDbContext(options);

        var result = await GetMealDetailByDateTool.GetMealDetailByDate(
            db, "2026-13-01", CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Contains("Invalid date format", result.ErrorMessage);
        Assert.Contains("2026-13-01", result.ErrorMessage);
    }

    [Fact]
    public async Task ReturnsErrorForEmptyDate()
    {
        var options = new DbContextOptionsBuilder<NutritionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        await using var db = new NutritionDbContext(options);

        var result = await GetMealDetailByDateTool.GetMealDetailByDate(
            db, "", CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Contains("Invalid date format", result.ErrorMessage);
    }
}
