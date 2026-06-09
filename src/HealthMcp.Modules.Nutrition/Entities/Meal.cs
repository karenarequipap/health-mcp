namespace HealthMcp.Modules.Nutrition.Entities;

public class Meal
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public int MealTypeId { get; set; }
    public MealType MealType { get; set; } = null!;
    public List<ConsumedProduct> ConsumedProducts { get; set; } = [];
}
