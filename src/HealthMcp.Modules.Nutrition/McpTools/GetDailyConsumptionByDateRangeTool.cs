using System.ComponentModel;
using HealthMcp.Modules.Nutrition.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;

namespace HealthMcp.Modules.Nutrition.McpTools;

[McpServerToolType]
public static class GetDailyConsumptionByDateRangeTool
{
    [McpServerTool, Description("Returns aggregated daily totals for energy, protein, fats, and carbohydrates within a date range")]
    public static async Task<DailyConsumptionResult> GetDailyConsumptionByDateRange(
        NutritionDbContext db,
        [Description("Start date (YYYY-MM-DD)")] string startDate,
        [Description("End date (YYYY-MM-DD)")] string endDate,
        CancellationToken cancellationToken)
    {
        var from = DateOnly.Parse(startDate);
        var to = DateOnly.Parse(endDate);

        var consumed = await db.ConsumedProducts
            .Where(c => c.Meal.Date >= from && c.Meal.Date <= to)
            .Select(c => new
            {
                c.Meal.Date,
                c.QuantityGrams,
                c.Product.Calories,
                c.Product.Protein,
                c.Product.Fats,
                c.Product.Carbohydrates
            })
            .ToListAsync(cancellationToken);

        var days = consumed
            .GroupBy(c => c.Date)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var factor = 1m / 100m;
                return new DaySummary(
                    g.Key.ToString("yyyy-MM-dd"),
                    Math.Round(g.Sum(x => x.Calories * x.QuantityGrams * factor), 2),
                    Math.Round(g.Sum(x => x.Protein * x.QuantityGrams * factor), 2),
                    Math.Round(g.Sum(x => x.Fats * x.QuantityGrams * factor), 2),
                    Math.Round(g.Sum(x => x.Carbohydrates * x.QuantityGrams * factor), 2)
                );
            })
            .ToList();

        return new DailyConsumptionResult(days);
    }
}

public record DaySummary(string date, decimal calories, decimal protein, decimal fats, decimal carbs);
public record DailyConsumptionResult(List<DaySummary> days);
