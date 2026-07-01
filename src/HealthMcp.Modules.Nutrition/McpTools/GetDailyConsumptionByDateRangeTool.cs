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

        var days = await db.ConsumedProducts
            .Where(c => c.Meal.Date >= from && c.Meal.Date <= to)
            .GroupBy(c => c.Meal.Date)
            .OrderBy(g => g.Key)
            .Select(g => new
            {
                Date = g.Key,
                Calories = g.Sum(x => x.Product.Calories * x.QuantityGrams / 100m),
                Protein = g.Sum(x => x.Product.Protein * x.QuantityGrams / 100m),
                Fats = g.Sum(x => x.Product.Fats * x.QuantityGrams / 100m),
                Carbs = g.Sum(x => x.Product.Carbohydrates * x.QuantityGrams / 100m)
            })
            .ToListAsync(cancellationToken);

        return new DailyConsumptionResult(days
            .Select(d => new DaySummary(
                d.Date.ToString("yyyy-MM-dd"),
                Math.Round(d.Calories, 2),
                Math.Round(d.Protein, 2),
                Math.Round(d.Fats, 2),
                Math.Round(d.Carbs, 2)
            ))
            .ToList());
    }
}

public record DaySummary(string date, decimal calories, decimal protein, decimal fats, decimal carbs);
public record DailyConsumptionResult(List<DaySummary> days);
