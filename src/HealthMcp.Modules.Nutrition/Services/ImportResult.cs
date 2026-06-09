namespace HealthMcp.Modules.Nutrition.Services;

public class ImportResult
{
    public int ProductsImported { get; set; }
    public int ProductsSkipped { get; set; }
    public int MealsCreated { get; set; }
    public int ConsumedProductsCreated { get; set; }
    public int ConsumedProductsSkipped { get; set; }
    public List<string> Errors { get; set; } = [];
}
