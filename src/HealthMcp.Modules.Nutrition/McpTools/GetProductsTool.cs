using System.ComponentModel;
using HealthMcp.Modules.Nutrition.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;

namespace HealthMcp.Modules.Nutrition.McpTools;

[McpServerToolType]
public static class GetProductsTool
{
    [McpServerTool, Description("Lists all products with their energy, protein, fats, and carbohydrates per 100g")]
    public static async Task<GetProductsResult> GetProducts(
        NutritionDbContext db,
        CancellationToken cancellationToken)
    {
        var products = await db.Products
            .OrderBy(p => p.Name)
            .Select(p => new ProductSummary(p.Name, p.Calories, p.Protein, p.Fats, p.Carbohydrates))
            .ToListAsync(cancellationToken);

        return new GetProductsResult(products);
    }
}

public record ProductSummary(string name, decimal energy, decimal protein, decimal fats, decimal carbs);
public record GetProductsResult(List<ProductSummary> products);
