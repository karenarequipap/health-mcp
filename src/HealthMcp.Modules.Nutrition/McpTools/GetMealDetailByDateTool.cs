using System.ComponentModel;
using HealthMcp.Modules.Nutrition.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;

namespace HealthMcp.Modules.Nutrition.McpTools;

[McpServerToolType]
public static class GetMealDetailByDateTool
{
    [McpServerTool, Description("Returns full meal breakdown including all products and their nutritional values for a specific date")]
    public static async Task<MealDetailResult> GetMealDetailByDate(
        NutritionDbContext db,
        [Description("Date (YYYY-MM-DD)")] string date,
        CancellationToken cancellationToken)
    {
        var d = DateOnly.Parse(date);

        var meals = await db.Meals
            .Include(m => m.MealType)
            .Include(m => m.ConsumedProducts)
                .ThenInclude(cp => cp.Product)
            .Where(m => m.Date == d)
            .OrderBy(m => m.MealType.Name)
            .ToListAsync(cancellationToken);

        var mealDetails = meals.Select(m => new MealDetail(
            m.MealType.Name,
            m.ConsumedProducts.Select(cp =>
            {
                var factor = cp.QuantityGrams / 100m;
                return new ProductConsumedDetail(
                    cp.Product.Name,
                    cp.QuantityGrams,
                    Math.Round(cp.Product.Calories * factor, 2)
                );
            }).ToList()
        )).ToList();

        return new MealDetailResult(mealDetails);
    }
}

public record ProductConsumedDetail(string name, decimal quantityGrams, decimal calories);
public record MealDetail(string mealType, List<ProductConsumedDetail> products);
public record MealDetailResult(List<MealDetail> meals);
