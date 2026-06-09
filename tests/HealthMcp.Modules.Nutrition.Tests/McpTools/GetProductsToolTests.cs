using HealthMcp.Modules.Nutrition.Entities;
using HealthMcp.Modules.Nutrition.Infrastructure;
using HealthMcp.Modules.Nutrition.McpTools;
using Microsoft.EntityFrameworkCore;

namespace HealthMcp.Modules.Nutrition.Tests.McpTools;

public class GetProductsToolTests
{
    [Fact]
    public async Task ReturnsAllProductsWithRequiredFields()
    {
        var options = new DbContextOptionsBuilder<NutritionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        await using var db = new NutritionDbContext(options);

        db.Products.AddRange(
            new Product { Name = "Eggs", Calories = 155, Protein = 13, Fats = 11, Carbohydrates = 1.1m },
            new Product { Name = "Feta cheese", Calories = 276, Protein = 16.5m, Fats = 23, Carbohydrates = 0.7m }
        );
        await db.SaveChangesAsync();

        var result = await GetProductsTool.GetProducts(db, CancellationToken.None);

        Assert.Equal(2, result.products.Count);
        var first = result.products[0];
        Assert.Equal("Eggs", first.name);
        Assert.Equal(155, first.energy);
        Assert.Equal(13, first.protein);
        Assert.Equal(11, first.fats);
        Assert.Equal(1.1m, first.carbs);
    }
}
