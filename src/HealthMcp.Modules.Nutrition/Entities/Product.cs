namespace HealthMcp.Modules.Nutrition.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Calories { get; set; }
    public decimal Protein { get; set; }
    public decimal Fats { get; set; }
    public decimal Carbohydrates { get; set; }

    public decimal? PlantProtein { get; set; }
    public decimal? AnimalProtein { get; set; }
    public decimal? Saturated { get; set; }
    public decimal? Monounsaturated { get; set; }
    public decimal? Polyunsaturated { get; set; }
    public decimal? Omega3 { get; set; }
    public decimal? Omega6 { get; set; }
    public decimal? Sugars { get; set; }
    public decimal? Cholesterol { get; set; }
    public decimal? Fibre { get; set; }
    public decimal? Caffeine { get; set; }
    public decimal? FolicAcid { get; set; }
    public decimal? VitaminA { get; set; }
    public decimal? VitaminB1 { get; set; }
    public decimal? VitaminB2 { get; set; }
    public decimal? VitaminB5 { get; set; }
    public decimal? VitaminB6 { get; set; }
    public decimal? Biotin { get; set; }
    public decimal? VitaminB12 { get; set; }
    public decimal? VitaminC { get; set; }
    public decimal? VitaminD { get; set; }
    public decimal? VitaminE { get; set; }
    public decimal? VitaminPP { get; set; }
    public decimal? VitaminK { get; set; }
    public decimal? Zinc { get; set; }
    public decimal? Phosphorous { get; set; }
    public decimal? Iodine { get; set; }
    public decimal? Magnesium { get; set; }
    public decimal? Copper { get; set; }
    public decimal? Potassium { get; set; }
    public decimal? Selenium { get; set; }
    public decimal? Sodium { get; set; }
    public decimal? Calcium { get; set; }
    public decimal? Iron { get; set; }
    public decimal? Salt { get; set; }
}
