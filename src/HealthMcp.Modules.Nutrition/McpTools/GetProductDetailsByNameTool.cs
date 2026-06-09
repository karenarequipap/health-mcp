using System.ComponentModel;
using HealthMcp.Modules.Nutrition.Entities;
using HealthMcp.Modules.Nutrition.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;

namespace HealthMcp.Modules.Nutrition.McpTools;

[McpServerToolType]
public static class GetProductDetailsByNameTool
{
    [McpServerTool, Description("Returns complete nutritional details for a product by name")]
    public static async Task<GetProductDetailsResult> GetProductDetailsByName(
        NutritionDbContext db,
        [Description("The exact product name to look up")] string productName,
        CancellationToken cancellationToken)
    {
        var product = await db.Products
            .FirstOrDefaultAsync(p => p.Name == productName, cancellationToken);

        return new GetProductDetailsResult(product);
    }
}

public record GetProductDetailsResult(Product? product);
