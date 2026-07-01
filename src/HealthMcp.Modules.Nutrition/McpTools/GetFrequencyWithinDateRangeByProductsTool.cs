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
        if (!DateOnly.TryParse(startDate, out var from))
        {
            return new FrequencyResult
            {
                IsError = true,
                ErrorMessage = $"Invalid start date format: '{startDate}'. Expected format: YYYY-MM-DD"
            };
        }

        if (!DateOnly.TryParse(endDate, out var to))
        {
            return new FrequencyResult
            {
                IsError = true,
                ErrorMessage = $"Invalid end date format: '{endDate}'. Expected format: YYYY-MM-DD"
            };
        }

        // Note: We intentionally fetch product names and group in memory because EF Core's
        // InMemory provider (used in tests) does not support server-side GroupBy translation.
        // With Npgsql (PostgreSQL), a server-side GROUP BY would be more efficient, but using
        // client-side grouping keeps the code portable across providers for this simple case.
        var consumed = await db.ConsumedProducts
            .Where(c => c.Meal.Date >= from && c.Meal.Date <= to)
            .Select(c => c.Product.Name)
            .ToListAsync(cancellationToken);

        var frequencies = consumed
            .GroupBy(name => name)
            .Select(g => new ProductFrequency(g.Key, g.Count()))
            .OrderByDescending(f => f.Count)
            .ToList();

        return new FrequencyResult { Products = frequencies };
    }
}

public record ProductFrequency(string Name, int Count);

public class FrequencyResult
{
    public List<ProductFrequency> Products { get; set; } = [];
    public bool IsError { get; set; }
    public string? ErrorMessage { get; set; }
}
