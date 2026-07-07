using System.ComponentModel;
using System.Linq.Expressions;
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
    private static readonly Dictionary<string, string> NutrientPropertyNames = new()
    {
        ["calories"] = "Calories",
        ["protein"] = "Protein",
        ["fats"] = "Fats",
        ["carbohydrates"] = "Carbohydrates",
        ["saturated"] = "Saturated",
        ["monounsaturated"] = "Monounsaturated",
        ["polyunsaturated"] = "Polyunsaturated",
        ["omega3"] = "Omega3",
        ["omega6"] = "Omega6",
        ["sugars"] = "Sugars",
        ["cholesterol"] = "Cholesterol",
        ["fibre"] = "Fibre",
        ["caffeine"] = "Caffeine",
        ["folicacid"] = "FolicAcid",
        ["vitamina"] = "VitaminA",
        ["vitaminb1"] = "VitaminB1",
        ["vitaminb2"] = "VitaminB2",
        ["vitaminb5"] = "VitaminB5",
        ["vitaminb6"] = "VitaminB6",
        ["biotin"] = "Biotin",
        ["vitaminb12"] = "VitaminB12",
        ["vitaminc"] = "VitaminC",
        ["vitamind"] = "VitaminD",
        ["vitamine"] = "VitaminE",
        ["vitaminpp"] = "VitaminPP",
        ["vitamink"] = "VitaminK",
        ["zinc"] = "Zinc",
        ["phosphorous"] = "Phosphorous",
        ["iodine"] = "Iodine",
        ["magnesium"] = "Magnesium",
        ["copper"] = "Copper",
        ["potassium"] = "Potassium",
        ["selenium"] = "Selenium",
        ["sodium"] = "Sodium",
        ["calcium"] = "Calcium",
        ["iron"] = "Iron",
        ["salt"] = "Salt",
    };

    /// <summary>
    /// Builds a dynamic expression: consumedProduct => ((consumedProduct.Product.<propertyName> ?? 0) * consumedProduct.QuantityGrams) / 100m
    /// Handles both nullable (decimal?) and non-nullable (decimal) property types.
    /// </summary>
    private static Expression<Func<ConsumedProduct, decimal>> BuildNutrientTotalExpression(string propertyName)
    {
        var param = Expression.Parameter(typeof(ConsumedProduct), "c");
        var product = Expression.Property(param, "Product");
        var property = Expression.Property(product, propertyName);

        Expression nutrientValue = property.Type.IsValueType && Nullable.GetUnderlyingType(property.Type) is null
            ? property
            : Expression.Coalesce(property, Expression.Constant(0m));

        var quantityGrams = Expression.Property(param, "QuantityGrams");
        var multiply = Expression.Multiply(nutrientValue, quantityGrams);
        var divide = Expression.Divide(multiply, Expression.Constant(100m));

        return Expression.Lambda<Func<ConsumedProduct, decimal>>(divide, param);
    }

    [McpServerTool, Description("Returns consumption of a specific nutrient within a date range. Mode can be 'aggregated' (total per day) or 'detailed' (grouped by product)")]
    public static async Task<NutrientResult> GetNutrientConsumptionByDateRange(
        NutritionDbContext db,
        [Description("Start date (YYYY-MM-DD)")] string startDate,
        [Description("End date (YYYY-MM-DD)")] string endDate,
        [Description("Nutrient name (e.g., protein, fats, saturated, sodium)")] string nutrient,
        [Description("Output mode: 'aggregated' or 'detailed'")] string mode,
        CancellationToken cancellationToken)
    {
        if (!NutrientPropertyNames.TryGetValue(nutrient.ToLowerInvariant(), out var propertyName))
        {
            return new NutrientResult
            {
                IsError = true,
                ErrorMessage = $"Unknown nutrient '{nutrient}'. Valid options: {string.Join(", ", NutrientPropertyNames.Keys.Order())}"
            };
        }

        var from = DateOnly.Parse(startDate);
        var to = DateOnly.Parse(endDate);

        var totalSelector = BuildNutrientTotalExpression(propertyName);

        var query = db.ConsumedProducts
            .Where(c => c.Meal.Date >= from && c.Meal.Date <= to);

        if (mode == "detailed")
        {
            var raw = await query
                .GroupBy(c => new { c.Meal.Date, c.Product.Name })
                .OrderBy(g => g.Key.Date)
                .ThenBy(g => g.Key.Name)
                .Select(g => new
                {
                    g.Key.Date,
                    g.Key.Name,
                    Value = g.AsQueryable().Sum(totalSelector)
                })
                .ToListAsync(cancellationToken);

            var days = raw
                .GroupBy(r => r.Date)
                .OrderBy(g => g.Key)
                .Select(g => new NutrientDaySummary
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Value = Math.Round(g.Sum(x => x.Value), 2),
                    Products = g.Select(x => new NutrientProductDetail
                    {
                        Name = x.Name,
                        Value = Math.Round(x.Value, 2)
                    }).OrderBy(x => x.Name).ToList()
                })
                .ToList();

            return new NutrientResult { Days = days };
        }

        var aggregated = await query
            .GroupBy(c => c.Meal.Date)
            .OrderBy(g => g.Key)
            .Select(g => new
            {
                Date = g.Key,
                Value = g.AsQueryable().Sum(totalSelector)
            })
            .ToListAsync(cancellationToken);

        return new NutrientResult
        {
            Days = aggregated.Select(d => new NutrientDaySummary
            {
                Date = d.Date.ToString("yyyy-MM-dd"),
                Value = Math.Round(d.Value, 2)
            }).ToList()
        };
    }
}
