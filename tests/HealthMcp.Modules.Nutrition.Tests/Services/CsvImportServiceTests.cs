using HealthMcp.Modules.Nutrition.Entities;
using HealthMcp.Modules.Nutrition.Infrastructure;
using HealthMcp.Modules.Nutrition.Services;
using Microsoft.EntityFrameworkCore;

namespace HealthMcp.Modules.Nutrition.Tests.Services;

public class CsvImportServiceTests
{
    private static NutritionDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<NutritionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new NutritionDbContext(options);
    }

    [Fact]
    public async Task ImportAsync_WithValidCsv_CreatesProductsAndMeals()
    {
        var db = CreateDbContext();
        var service = new CsvImportService(db);

        var csv = "Date,Meal,\"Products and dishes\",\"quantity (g)\",\"calories (kcal)\",\"Protein (g)\",\"Fats (g)\",\"Carbohydrates (g)\"\n"
                + "2026-02-01,Breakfast,\"Feta cheese\",6,16.56,0.99,1.38,0.042";

        var result = await service.ImportAsync(csv, CancellationToken.None);

        Assert.Equal(1, result.ProductsImported);
        Assert.Equal(0, result.ProductsSkipped);
        Assert.Equal(1, result.MealsCreated);
        Assert.Equal(1, result.ConsumedProductsCreated);
        Assert.Equal(0, result.ConsumedProductsSkipped);
        Assert.Empty(result.Errors);

        var product = await db.Products.FirstAsync();
        Assert.Equal("Feta cheese", product.Name);
        Assert.Equal(276.00m, product.Calories);
    }

    [Fact]
    public async Task ImportAsync_DuplicateProductNameAndCalories_SkipsProduct()
    {
        var db = CreateDbContext();
        var service = new CsvImportService(db);

        db.Products.Add(new Product { Name = "Feta cheese", Calories = 276, Protein = 16.5m, Fats = 23, Carbohydrates = 0.7m });
        await db.SaveChangesAsync();

        var csv = "Date,Meal,\"Products and dishes\",\"quantity (g)\",\"calories (kcal)\",\"Protein (g)\",\"Fats (g)\",\"Carbohydrates (g)\"\n"
                + "2026-02-01,Breakfast,\"Feta cheese\",6,16.56,0.99,1.38,0.042";

        var result = await service.ImportAsync(csv, CancellationToken.None);

        Assert.Equal(0, result.ProductsImported);
        Assert.Equal(1, result.ProductsSkipped);
    }

    [Fact]
    public async Task ImportAsync_DuplicateConsumedProduct_SkipsRow()
    {
        var db = CreateDbContext();
        var service = new CsvImportService(db);

        var csv = "Date,Meal,\"Products and dishes\",\"quantity (g)\",\"calories (kcal)\",\"Protein (g)\",\"Fats (g)\",\"Carbohydrates (g)\"\n"
                + "2026-02-01,Breakfast,\"Feta cheese\",6,16.56,0.99,1.38,0.042\n"
                + "2026-02-01,Breakfast,\"Feta cheese\",6,16.56,0.99,1.38,0.042";

        var result = await service.ImportAsync(csv, CancellationToken.None);

        Assert.Equal(1, result.ProductsImported);
        Assert.Equal(1, result.ProductsSkipped);
        Assert.Equal(1, result.MealsCreated);
        Assert.Equal(1, result.ConsumedProductsCreated);
        Assert.Equal(1, result.ConsumedProductsSkipped);
    }

    [Fact]
    public async Task ImportAsync_WithInvalidData_CollectsErrors()
    {
        var db = CreateDbContext();
        var service = new CsvImportService(db);

        var csv = "Date,Meal,\"Products and dishes\",\"quantity (g)\",\"calories (kcal)\",\"Protein (g)\",\"Fats (g)\",\"Carbohydrates (g)\"\n"
                + "invalid-date,Breakfast,\"Feta cheese\",6,16.56,0.99,1.38,0.042";

        var result = await service.ImportAsync(csv, CancellationToken.None);

        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task ImportAsync_MissingMealType_ReportsError()
    {
        var db = CreateDbContext();
        var service = new CsvImportService(db);

        var csv = "Date,Meal,\"Products and dishes\",\"quantity (g)\",\"calories (kcal)\",\"Protein (g)\",\"Fats (g)\",\"Carbohydrates (g)\"\n"
                + "2026-02-01,,\"Feta cheese\",6,16.56,0.99,1.38,0.042";

        var result = await service.ImportAsync(csv, CancellationToken.None);

        Assert.Single(result.Errors);
        Assert.Contains("meal type", result.Errors[0], StringComparison.OrdinalIgnoreCase);
    }
}
