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
        if (!DateOnly.TryParse(date, out var d))
        {
            return new MealDetailResult
            {
                IsError = true,
                ErrorMessage = $"Invalid date format: '{date}'. Expected format: YYYY-MM-DD"
            };
        }

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

        return new MealDetailResult { Meals = mealDetails };
    }
}

public record ProductConsumedDetail(string Name, decimal QuantityGrams, decimal Calories);
public record MealDetail(string MealType, List<ProductConsumedDetail> Products);

public class MealDetailResult
{
    public List<MealDetail> Meals { get; set; } = [];
    public bool IsError { get; set; }
    public string? ErrorMessage { get; set; }
}
