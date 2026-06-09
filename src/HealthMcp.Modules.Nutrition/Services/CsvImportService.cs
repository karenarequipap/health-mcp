using System.Globalization;
using HealthMcp.Modules.Nutrition.Entities;
using HealthMcp.Modules.Nutrition.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace HealthMcp.Modules.Nutrition.Services;

public class CsvImportService(NutritionDbContext db)
{
    public async Task<ImportResult> ImportAsync(string csvContent, CancellationToken ct)
    {
        var result = new ImportResult();

        var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2)
        {
            result.Errors.Add("CSV must contain a header row and at least one data row");
            return result;
        }

        var headers = ParseCsvLine(lines[0]);

        for (int i = 1; i < lines.Length; i++)
        {
            try
            {
                await ProcessRow(lines[i], headers, result, ct);
            }
            catch (Exception ex)
            {
                result.Errors.Add($"line {i + 1}: {ex.Message}");
            }
        }

        return result;
    }

    private async Task ProcessRow(string line, string[] headers, ImportResult result, CancellationToken ct)
    {
        var columns = ParseCsvLine(line);
        var values = headers.Zip(columns, (h, v) => new { h, v })
            .ToDictionary(x => x.h.Trim('"'), x => x.v.Trim('"'));

        var dateStr = values.GetValueOrDefault("Date", "");
        var mealName = values.GetValueOrDefault("Meal", "");
        var productName = values.GetValueOrDefault("Products and dishes", "");
        var quantityStr = values.GetValueOrDefault("quantity (g)", "");
        var caloriesStr = values.GetValueOrDefault("calories (kcal)", "");
        var proteinStr = values.GetValueOrDefault("Protein (g)", "");
        var fatsStr = values.GetValueOrDefault("Fats (g)", "");
        var carbsStr = values.GetValueOrDefault("Carbohydrates (g)", "");

        if (!DateOnly.TryParse(dateStr, CultureInfo.InvariantCulture, out var date))
            throw new FormatException($"invalid date '{dateStr}'");

        if (string.IsNullOrWhiteSpace(mealName))
            throw new FormatException("meal type is required and cannot be empty");

        if (!decimal.TryParse(quantityStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var quantity) || quantity <= 0)
            throw new FormatException($"invalid quantity '{quantityStr}'");

        var product = await GetOrCreateProduct(productName, values, result, ct);
        var mealType = await GetOrCreateMealType(mealName, ct);
        var meal = await GetOrCreateMeal(date, mealType.Id, result, ct);

        var exists = await db.ConsumedProducts
            .AnyAsync(c => c.MealId == meal.Id && c.ProductId == product.Id
                && c.QuantityGrams == quantity, ct);

        if (exists)
        {
            result.ConsumedProductsSkipped++;
            return;
        }

        db.ConsumedProducts.Add(new ConsumedProduct
        {
            MealId = meal.Id,
            ProductId = product.Id,
            QuantityGrams = quantity
        });

        await db.SaveChangesAsync(ct);
        result.ConsumedProductsCreated++;
    }

    private async Task<Product> GetOrCreateProduct(string name, Dictionary<string, string> values, ImportResult result, CancellationToken ct)
    {
        var quantityGrams = decimal.Parse(values["quantity (g)"], CultureInfo.InvariantCulture);
        var factor = 100m / quantityGrams;

        var scaledCalories = Math.Round((ParseNutrient(values, "calories (kcal)") ?? 0) * factor, 2);

        var existing = await db.Products
            .FirstOrDefaultAsync(p => p.Name == name && p.Calories == scaledCalories, ct);

        if (existing is not null)
        {
            result.ProductsSkipped++;
            return existing;
        }

        var product = new Product
        {
            Name = name,
            Calories = scaledCalories,
            Protein = Math.Round((ParseNutrient(values, "Protein (g)") ?? 0) * factor, 2),
            Fats = Math.Round((ParseNutrient(values, "Fats (g)") ?? 0) * factor, 2),
            Carbohydrates = Math.Round((ParseNutrient(values, "Carbohydrates (g)") ?? 0) * factor, 2),
        };

        MapOptionalNutrients(product, values, factor);

        db.Products.Add(product);
        await db.SaveChangesAsync(ct);
        result.ProductsImported++;
        return product;
    }

    private static void MapOptionalNutrients(Product product, Dictionary<string, string> values, decimal factor)
    {
        product.PlantProtein = MapOpt(values, "Plant (g)", factor);
        product.AnimalProtein = MapOpt(values, "Animal (g)", factor);
        product.Saturated = MapOpt(values, "Saturated (g)", factor);
        product.Monounsaturated = MapOpt(values, "Monounsaturated (g)", factor);
        product.Polyunsaturated = MapOpt(values, "Polyunsaturated (g)", factor);
        product.Omega3 = MapOpt(values, "Omega 3 fatty acid (g)", factor);
        product.Omega6 = MapOpt(values, "Omega 6 fatty acid (g)", factor);
        product.Sugars = MapOpt(values, "Sugars (g)", factor);
        product.Cholesterol = MapOpt(values, "Cholesterol (mg)", factor);
        product.Fibre = MapOpt(values, "Fibre (g)", factor);
        product.Caffeine = MapOpt(values, "Caffeine (mg)", factor);
        product.FolicAcid = MapOpt(values, "Folic acid (ug)", factor);
        product.VitaminA = MapOpt(values, "Vitamin A (ug)", factor);
        product.VitaminB1 = MapOpt(values, "Vitamin B1 (mg)", factor);
        product.VitaminB2 = MapOpt(values, "Vitamin B2 (mg)", factor);
        product.VitaminB5 = MapOpt(values, "Vitamin B5 (mg)", factor);
        product.VitaminB6 = MapOpt(values, "Vitamin B6 (mg)", factor);
        product.Biotin = MapOpt(values, "Biotin (ug)", factor);
        product.VitaminB12 = MapOpt(values, "Vitamin B12 (ug)", factor);
        product.VitaminC = MapOpt(values, "Vitamin C (mg)", factor);
        product.VitaminD = MapOpt(values, "Vitamin D (ug)", factor);
        product.VitaminE = MapOpt(values, "Vitamin E (mg)", factor);
        product.VitaminPP = MapOpt(values, "Vitamin PP (mg)", factor);
        product.VitaminK = MapOpt(values, "Vitamin K (ug)", factor);
        product.Zinc = MapOpt(values, "Zinc (mg)", factor);
        product.Phosphorous = MapOpt(values, "Phosphorous (mg)", factor);
        product.Iodine = MapOpt(values, "Iodine (ug)", factor);
        product.Magnesium = MapOpt(values, "Magnesium (mg)", factor);
        product.Copper = MapOpt(values, "Copper (mg)", factor);
        product.Potassium = MapOpt(values, "Potassium (mg)", factor);
        product.Selenium = MapOpt(values, "Selenium (ug)", factor);
        product.Sodium = MapOpt(values, "Sodium (mg)", factor);
        product.Calcium = MapOpt(values, "Calcium (mg)", factor);
        product.Iron = MapOpt(values, "Iron (mg)", factor);
        product.Salt = MapOpt(values, "Salt (g)", factor);
    }

    private static decimal? MapOpt(Dictionary<string, string> values, string key, decimal factor)
    {
        var val = ParseNutrient(values, key);
        return val.HasValue ? Math.Round(val.Value * factor, 2) : null;
    }

    private async Task<MealType> GetOrCreateMealType(string name, CancellationToken ct)
    {
        var existing = await db.MealTypes.FirstOrDefaultAsync(m => m.Name == name, ct);
        if (existing is not null) return existing;

        var mealType = new MealType { Name = name };
        db.MealTypes.Add(mealType);
        await db.SaveChangesAsync(ct);
        return mealType;
    }

    private async Task<Meal> GetOrCreateMeal(DateOnly date, int mealTypeId, ImportResult result, CancellationToken ct)
    {
        var existing = await db.Meals
            .FirstOrDefaultAsync(m => m.Date == date && m.MealTypeId == mealTypeId, ct);
        if (existing is not null) return existing;

        var meal = new Meal { Date = date, MealTypeId = mealTypeId };
        db.Meals.Add(meal);
        await db.SaveChangesAsync(ct);
        result.MealsCreated++;
        return meal;
    }

    private static decimal? ParseNutrient(Dictionary<string, string> values, string key)
    {
        if (!values.TryGetValue(key, out var val) || string.IsNullOrWhiteSpace(val))
            return null;
        if (decimal.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;
        return null;
    }

    private static string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        var current = new System.Text.StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        result.Add(current.ToString());
        return [.. result];
    }
}
