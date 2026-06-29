using System.ComponentModel;
using HealthMcp.Modules.Nutrition.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;

namespace HealthMcp.Modules.Nutrition.McpTools;

[McpServerToolType]
public static class GetFrequencyWithinDateRangeByProductsTool
{
    [McpServerTool, Description("Returns how many times each product was consumed within a date range")]
    public static async Task<FrequencyResult> GetFrequencyWithinDateRangeByProducts(
        NutritionDbContext db,
        [Description("Start date (YYYY-MM-DD)")] string startDate,
        [Description("End date (YYYY-MM-DD)")] string endDate,
        CancellationToken cancellationToken)
    {
        var from = DateOnly.Parse(startDate);
        var to = DateOnly.Parse(endDate);

        var consumed = await db.ConsumedProducts
            .Where(c => c.Meal.Date >= from && c.Meal.Date <= to)
            .Select(c => c.Product.Name)
            .ToListAsync(cancellationToken);

        var frequencies = consumed
            .GroupBy(name => name)
            .Select(g => new ProductFrequency(g.Key, g.Count()))
            .OrderByDescending(f => f.count)
            .ToList();

        return new FrequencyResult(frequencies);
    }
}

public record ProductFrequency(string name, int count);
public record FrequencyResult(List<ProductFrequency> products);
