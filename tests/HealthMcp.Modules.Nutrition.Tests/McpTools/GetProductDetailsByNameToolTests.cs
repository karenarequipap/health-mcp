using HealthMcp.Modules.Nutrition.Entities;
using HealthMcp.Modules.Nutrition.Infrastructure;
using HealthMcp.Modules.Nutrition.McpTools;
using Microsoft.EntityFrameworkCore;

namespace HealthMcp.Modules.Nutrition.Tests.McpTools;

public class GetProductDetailsByNameToolTests
{
    [Fact]
    public async Task ReturnsProductDetailsWhenFound()
    {
        var options = new DbContextOptionsBuilder<NutritionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        await using var db = new NutritionDbContext(options);

        db.Products.Add(new Product
        {
            Name = "Feta cheese",
            Calories = 276,
            Protein = 16.5m,
            Fats = 23,
            Carbohydrates = 0.7m,
            Saturated = 16.1m,
            Sodium = 1116
        });
        await db.SaveChangesAsync();

        var result = await GetProductDetailsByNameTool.GetProductDetailsByName(db, "Feta cheese", CancellationToken.None);

        Assert.NotNull(result.product);
        Assert.Equal("Feta cheese", result.product.Name);
        Assert.Equal(276, result.product.Calories);
        Assert.Equal(16.1m, result.product.Saturated);
    }

    [Fact]
    public async Task ReturnsNullWhenProductNotFound()
    {
        var options = new DbContextOptionsBuilder<NutritionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        await using var db = new NutritionDbContext(options);

        var result = await GetProductDetailsByNameTool.GetProductDetailsByName(db, "Nonexistent", CancellationToken.None);

        Assert.Null(result.product);
    }
}
