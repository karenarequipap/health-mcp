using System.ComponentModel;
using HealthMcp.Modules.Nutrition.Entities;
using HealthMcp.Modules.Nutrition.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;

namespace HealthMcp.Modules.Nutrition.McpTools;

public class NutrientResult
{
    public List<NutrientDaySummary> Days { get; set; } = [];
    public bool IsError { get; set; }
    public string? ErrorMessage { get; set; }
}

public class NutrientDaySummary
{
    public string Date { get; set; } = "";
    public decimal Value { get; set; }
    public List<NutrientProductDetail>? Products { get; set; }
}

public class NutrientProductDetail
{
    public string Name { get; set; } = "";
    public decimal Value { get; set; }
}

[McpServerToolType]
public static class GetNutrientConsumptionByDateRangeTool
{
    private static readonly Dictionary<string, Func<Product, decimal?>> NutrientMap = new()
    {
        ["calories"] = p => p.Calories,
        ["protein"] = p => p.Protein,
        ["fats"] = p => p.Fats,
        ["carbohydrates"] = p => p.Carbohydrates,
        ["saturated"] = p => p.Saturated,
        ["monounsaturated"] = p => p.Monounsaturated,
        ["polyunsaturated"] = p => p.Polyunsaturated,
        ["omega3"] = p => p.Omega3,
        ["omega6"] = p => p.Omega6,
        ["sugars"] = p => p.Sugars,
        ["cholesterol"] = p => p.Cholesterol,
        ["fibre"] = p => p.Fibre,
        ["caffeine"] = p => p.Caffeine,
        ["folicacid"] = p => p.FolicAcid,
        ["vitamina"] = p => p.VitaminA,
        ["vitaminb1"] = p => p.VitaminB1,
        ["vitaminb2"] = p => p.VitaminB2,
        ["vitaminb5"] = p => p.VitaminB5,
        ["vitaminb6"] = p => p.VitaminB6,
        ["biotin"] = p => p.Biotin,
        ["vitaminb12"] = p => p.VitaminB12,
        ["vitaminc"] = p => p.VitaminC,
        ["vitamind"] = p => p.VitaminD,
        ["vitamine"] = p => p.VitaminE,
        ["vitaminpp"] = p => p.VitaminPP,
        ["vitamink"] = p => p.VitaminK,
        ["zinc"] = p => p.Zinc,
        ["phosphorous"] = p => p.Phosphorous,
        ["iodine"] = p => p.Iodine,
        ["magnesium"] = p => p.Magnesium,
        ["copper"] = p => p.Copper,
        ["potassium"] = p => p.Potassium,
        ["selenium"] = p => p.Selenium,
        ["sodium"] = p => p.Sodium,
        ["calcium"] = p => p.Calcium,
        ["iron"] = p => p.Iron,
        ["salt"] = p => p.Salt,
    };

    [McpServerTool, Description("Returns consumption of a specific nutrient within a date range. Mode can be 'aggregated' (total per day) or 'detailed' (grouped by product)")]
    public static async Task<NutrientResult> GetNutrientConsumptionByDateRange(
        NutritionDbContext db,
        [Description("Start date (YYYY-MM-DD)")] string startDate,
        [Description("End date (YYYY-MM-DD)")] string endDate,
        [Description("Nutrient name (e.g., protein, fats, saturated, sodium)")] string nutrient,
        [Description("Output mode: 'aggregated' or 'detailed'")] string mode,
        CancellationToken cancellationToken)
    {
        if (!NutrientMap.TryGetValue(nutrient.ToLowerInvariant(), out var selector))
        {
            return new NutrientResult
            {
                IsError = true,
                ErrorMessage = $"Unknown nutrient '{nutrient}'. Valid options: {string.Join(", ", NutrientMap.Keys.Order())}"
            };
        }

        var from = DateOnly.Parse(startDate);
        var to = DateOnly.Parse(endDate);

        var consumed = await db.ConsumedProducts
            .Where(c => c.Meal.Date >= from && c.Meal.Date <= to)
            .Select(c => new
            {
                c.Meal.Date,
                c.Product.Name,
                c.QuantityGrams,
                Product = c.Product
            })
            .ToListAsync(cancellationToken);

        if (mode == "detailed")
        {
            var days = consumed
                .GroupBy(c => c.Date)
                .OrderBy(g => g.Key)
                .Select(g => new NutrientDaySummary
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Products = g
                        .GroupBy(x => x.Name)
                        .Select(x => new NutrientProductDetail
                        {
                            Name = x.Key,
                            Value = Math.Round(x.Sum(p => (selector(p.Product) ?? 0) * p.QuantityGrams / 100m), 2)
                        })
                        .OrderBy(x => x.Name)
                        .ToList(),
                    Value = Math.Round(g.Sum(x => (selector(x.Product) ?? 0) * x.QuantityGrams / 100m), 2)
                })
                .ToList();

            return new NutrientResult { Days = days };
        }

        var aggregated = consumed
            .GroupBy(c => c.Date)
            .OrderBy(g => g.Key)
            .Select(g => new NutrientDaySummary
            {
                Date = g.Key.ToString("yyyy-MM-dd"),
                Value = Math.Round(g.Sum(x => (selector(x.Product) ?? 0) * x.QuantityGrams / 100m), 2)
            })
            .ToList();

        return new NutrientResult { Days = aggregated };
    }
}
