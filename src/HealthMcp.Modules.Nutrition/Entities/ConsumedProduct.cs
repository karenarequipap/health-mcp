namespace HealthMcp.Modules.Nutrition.Entities;

public class ConsumedProduct
{
    public int Id { get; set; }
    public int MealId { get; set; }
    public Meal Meal { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public decimal QuantityGrams { get; set; }
}
