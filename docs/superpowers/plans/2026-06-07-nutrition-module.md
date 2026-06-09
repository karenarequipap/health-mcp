# Nutrition Module — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement the Nutrition module for the Health MCP server — entities, CSV import API, and MCP query tools.

**Architecture:** Modular monolith with four projects — Nutrition module (entities + EF + tools), API (CSV import), MCP Server (query tools), and tests. API runs on port 9092, MCP Server on port 9093. Both share a PostgreSQL database via EF Core and reference the same Nutrition module library.

**Tech Stack:** .NET 10 LTS, ModelContextProtocol.AspNetCore SDK, EF Core 10 + Npgsql, PostgreSQL 16, xUnit, Moq

---

### Task 1: Install Prerequisites — **DONE**

**Files:** none

- [x] **Step 1: Install .NET 10 SDK via brew**
- [x] **Step 2: Create docker-compose.yml for PostgreSQL**
- [x] **Step 3: Start PostgreSQL via Docker and create database**
- [x] **Step 4: Commit**

---

### Task 2: Scaffold Solution and Projects — **DONE**

**Files:**
- Create: `HealthMcp.slnx` (`.slnx` format)
- Create: `src/HealthMcp.Modules.Nutrition/HealthMcp.Modules.Nutrition.csproj`
- Create: `src/HealthMcp.Api/HealthMcp.Api.csproj`
- Create: `src/HealthMcp.McpServer/HealthMcp.McpServer.csproj`
- Create: `tests/HealthMcp.Modules.Nutrition.Tests/HealthMcp.Modules.Nutrition.Tests.csproj`
- Create: `tests/HealthMcp.Api.Tests/HealthMcp.Api.Tests.csproj`

- [x] **Step 1: Create solution and projects**
- [x] **Step 2: Add project references**
- [x] **Step 3: Add NuGet packages to Nutrition module**
- [x] **Step 4: Add NuGet packages to MCP Server**
- [x] **Step 5: Add NuGet packages to test projects**
- [x] **Step 6: Create directory structure**
- [x] **Step 7: Verify build**
- [x] **Step 8: Commit**

---

### Task 3: Create Domain Entities

**Files:**
- Create: `src/HealthMcp.Modules.Nutrition/Entities/Product.cs`
- Create: `src/HealthMcp.Modules.Nutrition/Entities/MealType.cs`
- Create: `src/HealthMcp.Modules.Nutrition/Entities/Meal.cs`
- Create: `src/HealthMcp.Modules.Nutrition/Entities/ConsumedProduct.cs`

- [x] **Step 1: Create Product.cs**

```csharp
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
```

- [x] **Step 2: Create MealType.cs**

```csharp
namespace HealthMcp.Modules.Nutrition.Entities;

public class MealType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
```

- [x] **Step 3: Create Meal.cs**

```csharp
namespace HealthMcp.Modules.Nutrition.Entities;

public class Meal
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public int MealTypeId { get; set; }
    public MealType MealType { get; set; } = null!;
    public List<ConsumedProduct> ConsumedProducts { get; set; } = [];
}
```

- [x] **Step 4: Create ConsumedProduct.cs**

```csharp
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
```

- [x] **Step 5: Build and verify**

```bash
dotnet build src/HealthMcp.Modules.Nutrition/
# Expected: Build succeeded
```

- [x] **Step 6: Commit**

```bash
git add -A
git commit -m "feat: add domain entities for Nutrition module"
```

---

### Task 4: Create EF Configurations and DbContext

**Files:**
- Create: `src/HealthMcp.Modules.Nutrition/Infrastructure/Configurations/ProductConfiguration.cs`
- Create: `src/HealthMcp.Modules.Nutrition/Infrastructure/Configurations/MealTypeConfiguration.cs`
- Create: `src/HealthMcp.Modules.Nutrition/Infrastructure/Configurations/MealConfiguration.cs`
- Create: `src/HealthMcp.Modules.Nutrition/Infrastructure/Configurations/ConsumedProductConfiguration.cs`
- Create: `src/HealthMcp.Modules.Nutrition/Infrastructure/NutritionDbContext.cs`

- [x] **Step 1: Create ProductConfiguration.cs**

```csharp
using HealthMcp.Modules.Nutrition.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthMcp.Modules.Nutrition.Infrastructure.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(500);
        builder.Property(p => p.Calories).IsRequired();
        builder.Property(p => p.Protein).IsRequired();
        builder.Property(p => p.Fats).IsRequired();
        builder.Property(p => p.Carbohydrates).IsRequired();

        builder.HasIndex(p => new { p.Name, p.Calories }).IsUnique();
    }
}
```

- [x] **Step 2: Create MealTypeConfiguration.cs**

```csharp
using HealthMcp.Modules.Nutrition.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthMcp.Modules.Nutrition.Infrastructure.Configurations;

public class MealTypeConfiguration : IEntityTypeConfiguration<MealType>
{
    public void Configure(EntityTypeBuilder<MealType> builder)
    {
        builder.ToTable("MealTypes");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(m => m.Name).IsUnique();
    }
}
```

- [x] **Step 3: Create MealConfiguration.cs**

```csharp
using HealthMcp.Modules.Nutrition.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthMcp.Modules.Nutrition.Infrastructure.Configurations;

public class MealConfiguration : IEntityTypeConfiguration<Meal>
{
    public void Configure(EntityTypeBuilder<Meal> builder)
    {
        builder.ToTable("Meals");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Date).IsRequired();

        builder.HasOne(m => m.MealType)
            .WithMany()
            .HasForeignKey(m => m.MealTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => new { m.Date, m.MealTypeId }).IsUnique();
    }
}
```

- [x] **Step 4: Create ConsumedProductConfiguration.cs**

```csharp
using HealthMcp.Modules.Nutrition.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthMcp.Modules.Nutrition.Infrastructure.Configurations;

public class ConsumedProductConfiguration : IEntityTypeConfiguration<ConsumedProduct>
{
    public void Configure(EntityTypeBuilder<ConsumedProduct> builder)
    {
        builder.ToTable("ConsumedProducts");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.QuantityGrams).IsRequired();

        builder.HasOne(c => c.Meal)
            .WithMany(m => m.ConsumedProducts)
            .HasForeignKey(c => c.MealId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Product)
            .WithMany()
            .HasForeignKey(c => c.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.MealId, c.ProductId, c.QuantityGrams }).IsUnique();
    }
}
```

- [x] **Step 5: Create NutritionDbContext.cs**

```csharp
using HealthMcp.Modules.Nutrition.Entities;
using HealthMcp.Modules.Nutrition.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace HealthMcp.Modules.Nutrition.Infrastructure;

public class NutritionDbContext : DbContext
{
    public NutritionDbContext(DbContextOptions<NutritionDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<MealType> MealTypes => Set<MealType>();
    public DbSet<Meal> Meals => Set<Meal>();
    public DbSet<ConsumedProduct> ConsumedProducts => Set<ConsumedProduct>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new MealTypeConfiguration());
        modelBuilder.ApplyConfiguration(new MealConfiguration());
        modelBuilder.ApplyConfiguration(new ConsumedProductConfiguration());
    }
}
```

- [x] **Step 6: Build and verify**

```bash
dotnet build src/HealthMcp.Modules.Nutrition/
# Expected: Build succeeded
```

- [x] **Step 7: Commit**

```bash
git add -A
git commit -m "feat: add EF Core configurations and NutritionDbContext"
```

---

### Task 5: Create and Apply Initial Migration — **DONE**

**Files:**
- Modify: `src/HealthMcp.Api/appsettings.json` (connection string)

- [x] **Step 1: Add EF tooling and connection string**

```bash
dotnet new tool-manifest
dotnet tool install dotnet-ef
```

- [x] **Step 2: Set connection string in API project**

Create `src/HealthMcp.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=health_mcp;Username=postgres;Password=postgres"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

- [x] **Step 3: Add DbContext registration to API Program.cs**

Write `src/HealthMcp.Api/Program.cs`:

```csharp
using HealthMcp.Modules.Nutrition.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<NutritionDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

app.Run();
```

- [x] **Step 4: Create initial migration**

```bash
dotnet ef migrations add InitialCreate --project src/HealthMcp.Modules.Nutrition --startup-project src/HealthMcp.Api
# Expected: Build succeeded, migration created in src/HealthMcp.Modules.Nutrition/Migrations/
```

- [x] **Step 5: Apply migration to database**

```bash
dotnet ef database update --project src/HealthMcp.Modules.Nutrition --startup-project src/HealthMcp.Api
# Expected: Applying migration 'InitialCreate'. Done.
```

- [x] **Step 6: Verify tables exist**

```bash
docker compose exec postgres psql -U postgres -d health_mcp -c "\dt"
# Expected: Tables listed: Products, MealTypes, Meals, ConsumedProducts
```

- [x] **Step 7: Commit**

```bash
git add -A
git commit -m "feat: create and apply initial EF migration"
```

---

### Task 6: CSV Import Service (TDD)

**Files:**
- Create: `tests/HealthMcp.Modules.Nutrition.Tests/Services/CsvImportServiceTests.cs`
- Create: `src/HealthMcp.Modules.Nutrition/Services/CsvImportService.cs`
- Create: `src/HealthMcp.Modules.Nutrition/Services/ImportResult.cs`

- [x] **Step 1: Create the ImportResult DTO**

Write `src/HealthMcp.Modules.Nutrition/Services/ImportResult.cs`:

```csharp
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
```

- [x] **Step 2: Write the failing test**

Write `tests/HealthMcp.Modules.Nutrition.Tests/Services/CsvImportServiceTests.cs`:

```csharp
using HealthMcp.Modules.Nutrition.Entities;
using HealthMcp.Modules.Nutrition.Infrastructure;
using HealthMcp.Modules.Nutrition.Services;
using Microsoft.EntityFrameworkCore;

namespace HealthMcp.Modules.Nutrition.Tests.Services;

public class CsvImportServiceTests
{
    private static NutritionDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<NutritionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new NutritionDbContext(options);
    }

    [Fact]
    public async Task ImportAsync_WithValidCsv_CreatesProductsAndMeals()
    {
        var db = CreateDbContext();
        var service = new CsvImportService(db);

        var csv = "Date,Meal,\"Products and dishes\",\"quantity (g)\",\"calories (kcal)\",\"Protein (g)\",\"Fats (g)\",\"Carbohydrates (g)\"\n"
                + "2026-02-01,Breakfast,\"Feta cheese\",6,16.56,0.99,1.38,0.042";

        var result = await service.ImportAsync(csv, CancellationToken.None);

        Assert.Equal(1, result.ProductsImported);
        Assert.Equal(0, result.ProductsSkipped);
        Assert.Equal(1, result.MealsCreated);
        Assert.Equal(1, result.ConsumedProductsCreated);
        Assert.Equal(0, result.ConsumedProductsSkipped);
        Assert.Empty(result.Errors);

        var product = await db.Products.FirstAsync();
        Assert.Equal("Feta cheese", product.Name);
        Assert.Equal(16.56m, product.Calories);
        // Note: values are per quantity in CSV (6g), stored per 100g:
        // calories per 100g = 16.56 * 100 / 6 = 276
    }

    [Fact]
    public async Task ImportAsync_DuplicateProductNameAndCalories_SkipsProduct()
    {
        var db = CreateDbContext();
        var service = new CsvImportService(db);

        db.Products.Add(new Product { Name = "Feta cheese", Calories = 276, Protein = 16.5m, Fats = 23, Carbohydrates = 0.7m });
        await db.SaveChangesAsync();

        var csv = "Date,Meal,\"Products and dishes\",\"quantity (g)\",\"calories (kcal)\",\"Protein (g)\",\"Fats (g)\",\"Carbohydrates (g)\"\n"
                + "2026-02-01,Breakfast,\"Feta cheese\",6,16.56,0.99,1.38,0.042";

        var result = await service.ImportAsync(csv, CancellationToken.None);

        Assert.Equal(0, result.ProductsImported);
        Assert.Equal(1, result.ProductsSkipped);
    }

    [Fact]
    public async Task ImportAsync_DuplicateConsumedProduct_SkipsRow()
    {
        var db = CreateDbContext();
        var service = new CsvImportService(db);

        var csv = "Date,Meal,\"Products and dishes\",\"quantity (g)\",\"calories (kcal)\",\"Protein (g)\",\"Fats (g)\",\"Carbohydrates (g)\"\n"
                + "2026-02-01,Breakfast,\"Feta cheese\",6,16.56,0.99,1.38,0.042\n"
                + "2026-02-01,Breakfast,\"Feta cheese\",6,16.56,0.99,1.38,0.042";

        var result = await service.ImportAsync(csv, CancellationToken.None);

        Assert.Equal(1, result.ProductsImported);
        Assert.Equal(1, result.ProductsSkipped);
        Assert.Equal(1, result.MealsCreated);
        Assert.Equal(1, result.ConsumedProductsCreated);
        Assert.Equal(1, result.ConsumedProductsSkipped);
    }

    [Fact]
    public async Task ImportAsync_WithInvalidData_CollectsErrors()
    {
        var db = CreateDbContext();
        var service = new CsvImportService(db);

        var csv = "Date,Meal,\"Products and dishes\",\"quantity (g)\",\"calories (kcal)\",\"Protein (g)\",\"Fats (g)\",\"Carbohydrates (g)\"\n"
                + "invalid-date,Breakfast,\"Feta cheese\",6,16.56,0.99,1.38,0.042";

        var result = await service.ImportAsync(csv, CancellationToken.None);

        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task ImportAsync_MissingMealType_ReportsError()
    {
        var db = CreateDbContext();
        var service = new CsvImportService(db);

        var csv = "Date,Meal,\"Products and dishes\",\"quantity (g)\",\"calories (kcal)\",\"Protein (g)\",\"Fats (g)\",\"Carbohydrates (g)\"\n"
                + "2026-02-01,,\"Feta cheese\",6,16.56,0.99,1.38,0.042";

        var result = await service.ImportAsync(csv, CancellationToken.None);

        Assert.Single(result.Errors);
        Assert.Contains("meal type", result.Errors[0], StringComparison.OrdinalIgnoreCase);
    }
}
```

- [x] **Step 3: Run test to verify it fails**

```bash
dotnet test tests/HealthMcp.Modules.Nutrition.Tests/ --filter "FullyQualifiedName~CsvImportServiceTests"
# Expected: Build succeeded, 0 passed, 4 failed (CsvImportService not found)
```

- [x] **Step 4: Write minimal implementation**

Write `src/HealthMcp.Modules.Nutrition/Services/CsvImportService.cs`:

```csharp
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
        var meal = await GetOrCreateMeal(date, mealType.Id, ct);

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
        var caloriesPer100 = ParseNutrient(values, "calories (kcal)") ?? 0;

        var existing = await db.Products
            .FirstOrDefaultAsync(p => p.Name == name && p.Calories == caloriesPer100, ct);

        if (existing is not null)
        {
            result.ProductsSkipped++;
            return existing;
        }

        var quantityGrams = decimal.Parse(values["quantity (g)"], CultureInfo.InvariantCulture);
        var factor = 100m / quantityGrams;

        var product = new Product
        {
            Name = name,
            Calories = Math.Round(ParseNutrient(values, "calories (kcal)")!.Value * factor, 2),
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

    private async Task<Meal> GetOrCreateMeal(DateOnly date, int mealTypeId, CancellationToken ct)
    {
        var existing = await db.Meals
            .FirstOrDefaultAsync(m => m.Date == date && m.MealTypeId == mealTypeId, ct);
        if (existing is not null) return existing;

        var meal = new Meal { Date = date, MealTypeId = mealTypeId };
        db.Meals.Add(meal);
        await db.SaveChangesAsync(ct);
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
```

- [x] **Step 5: Fix the test assertion — CSV values are per consumed quantity, need scaling to per 100g**

Update the first test's assertion in `CsvImportServiceTests.cs`:

The product's CSV says: 6g of feta cheese has 16.56 kcal. Per 100g: `16.56 * 100 / 6 = 276`. So the product entity should store 276 kcal per 100g.

Find and replace the assertion block:

Old:
```csharp
        var product = await db.Products.FirstAsync();
        Assert.Equal("Feta cheese", product.Name);
        Assert.Equal(16.56m, product.Calories);
        // Note: values are per quantity in CSV (6g), stored per 100g:
        // calories per 100g = 16.56 * 100 / 6 = 276
```

New:
```csharp
        var product = await db.Products.FirstAsync();
        Assert.Equal("Feta cheese", product.Name);
        Assert.Equal(276.00m, product.Calories);
```

- [x] **Step 6: Run tests to verify they pass**

```bash
dotnet test tests/HealthMcp.Modules.Nutrition.Tests/ --filter "FullyQualifiedName~CsvImportServiceTests"
# Expected: Passed 5, Failed 0
```

If tests fail, debug the service implementation and re-run until they pass.

- [x] **Step 7: Commit**

```bash
git add -A
git commit -m "feat: implement CSV import service with duplicate detection"
```

---

### Task 7: CSV Import API Endpoint

**Files:**
- Create: `src/HealthMcp.Api/Endpoints/CsvImportEndpoints.cs`
- Modify: `src/HealthMcp.Api/Program.cs`

- [x] **Step 1: Write the failing integration test**

Write `tests/HealthMcp.Api.Tests/CsvImportEndpointTests.cs`:

```csharp
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HealthMcp.Api.Tests;

public class CsvImportEndpointTests
{
    [Fact]
    public async Task PostCsvImport_ReturnsOkWithSummary()
    {
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        var csv = "Date,Meal,\"Products and dishes\",\"quantity (g)\",\"calories (kcal)\",\"Protein (g)\",\"Fats (g)\",\"Carbohydrates (g)\"\n"
                + "2026-02-01,Breakfast,\"Feta cheese\",6,16.56,0.99,1.38,0.042";

        var content = new StringContent(csv, Encoding.UTF8, "text/csv");
        var response = await client.PostAsync("/api/import/csv", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
```

- [x] **Step 2: Run test to verify it fails**

```bash
dotnet test tests/HealthMcp.Api.Tests/
# Expected: Fails (endpoint not implemented)
```

- [x] **Step 3: Implement the endpoint**

Write `src/HealthMcp.Api/Endpoints/CsvImportEndpoints.cs`:

```csharp
using HealthMcp.Modules.Nutrition.Services;

namespace HealthMcp.Api.Endpoints;

public static class CsvImportEndpoints
{
    public static void MapCsvImportEndpoints(this WebApplication app)
    {
        app.MapPost("/api/import/csv", async (HttpRequest request, CsvImportService service, CancellationToken ct) =>
        {
            using var reader = new StreamReader(request.Body);
            var csvContent = await reader.ReadToEndAsync(ct);
            var result = await service.ImportAsync(csvContent, ct);
            return Results.Ok(result);
        })
        .Accepts<string>("text/csv");
    }
}
```

- [x] **Step 4: Update Program.cs**

Write `src/HealthMcp.Api/Program.cs`:

```csharp
using HealthMcp.Api.Endpoints;
using HealthMcp.Modules.Nutrition.Infrastructure;
using HealthMcp.Modules.Nutrition.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<NutritionDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<CsvImportService>();

var app = builder.Build();

app.MapCsvImportEndpoints();

app.Run();
```

Note: The `Program` class needs to be accessible from the test project. Add at the end of file:

```csharp
public partial class Program { }
```

- [x] **Step 5: Configure API port**

Update `src/HealthMcp.Api/Properties/launchSettings.json` or add to `appsettings.json`:

```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:9092"
      }
    }
  }
}
```

- [x] **Step 6: Run tests to verify they pass**

```bash
dotnet test tests/HealthMcp.Api.Tests/
# Expected: Passed
```

- [x] **Step 7: Manual smoke test**

```bash
# Start API in background
dotnet run --project src/HealthMcp.Api &
sleep 3

# Test with sample CSV
curl -X POST -H "Content-Type: text/csv" --data-binary "Date,Meal,\"Products and dishes\",\"quantity (g)\",\"calories (kcal)\",\"Protein (g)\",\"Fats (g)\",\"Carbohydrates (g)\"
2026-02-01,Breakfast,\"Feta cheese\",6,16.56,0.99,1.38,0.042" http://localhost:9092/api/import/csv

# Kill the process
kill %1
```

- [x] **Step 8: Commit**

```bash
git add -A
git commit -m "feat: add CSV import API endpoint on port 9092"
```

---

### Task 8: MCP Tools — get_products and get_product_details_by_name (TDD)

**Files:**
- Create: `tests/HealthMcp.Modules.Nutrition.Tests/McpTools/GetProductsToolTests.cs`
- Create: `src/HealthMcp.Modules.Nutrition/McpTools/GetProductsTool.cs`
- Create: `tests/HealthMcp.Modules.Nutrition.Tests/McpTools/GetProductDetailsByNameToolTests.cs`
- Create: `src/HealthMcp.Modules.Nutrition/McpTools/GetProductDetailsByNameTool.cs`

- [x] **Step 1: Write failing test for get_products**

Write `tests/HealthMcp.Modules.Nutrition.Tests/McpTools/GetProductsToolTests.cs`:

```csharp
using HealthMcp.Modules.Nutrition.Entities;
using HealthMcp.Modules.Nutrition.Infrastructure;
using HealthMcp.Modules.Nutrition.McpTools;
using Microsoft.EntityFrameworkCore;

namespace HealthMcp.Modules.Nutrition.Tests.McpTools;

public class GetProductsToolTests
{
    [Fact]
    public async Task ReturnsAllProductsWithRequiredFields()
    {
        var options = new DbContextOptionsBuilder<NutritionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        await using var db = new NutritionDbContext(options);

        db.Products.AddRange(
            new Product { Name = "Eggs", Calories = 155, Protein = 13, Fats = 11, Carbohydrates = 1.1m },
            new Product { Name = "Feta cheese", Calories = 276, Protein = 16.5m, Fats = 23, Carbohydrates = 0.7m }
        );
        await db.SaveChangesAsync();

        var result = await GetProductsTool.GetProducts(db, CancellationToken.None);

        Assert.Equal(2, result.products.Count);
        var first = result.products[0];
        Assert.Equal("Eggs", first.name);
        Assert.Equal(155, first.energy);
        Assert.Equal(13, first.protein);
        Assert.Equal(11, first.fats);
        Assert.Equal(1.1m, first.carbs);
    }
}
```

- [x] **Step 2: Run test to verify it fails**

```bash
dotnet test tests/HealthMcp.Modules.Nutrition.Tests/ --filter "FullyQualifiedName~GetProductsToolTests"
# Expected: Fails
```

- [x] **Step 3: Implement get_products tool**

Write `src/HealthMcp.Modules.Nutrition/McpTools/GetProductsTool.cs`:

```csharp
using System.ComponentModel;
using HealthMcp.Modules.Nutrition.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;

namespace HealthMcp.Modules.Nutrition.McpTools;

[McpServerToolType]
public static class GetProductsTool
{
    [McpServerTool, Description("Lists all products with their energy, protein, fats, and carbohydrates per 100g")]
    public static async Task<GetProductsResult> GetProducts(
        NutritionDbContext db,
        CancellationToken cancellationToken)
    {
        var products = await db.Products
            .OrderBy(p => p.Name)
            .Select(p => new ProductSummary(p.Name, p.Calories, p.Protein, p.Fats, p.Carbohydrates))
            .ToListAsync(cancellationToken);

        return new GetProductsResult(products);
    }
}

public record ProductSummary(string name, decimal energy, decimal protein, decimal fats, decimal carbs);
public record GetProductsResult(List<ProductSummary> products);
```

- [x] **Step 4: Run tests to verify they pass**

```bash
dotnet test tests/HealthMcp.Modules.Nutrition.Tests/ --filter "FullyQualifiedName~GetProductsToolTests"
# Expected: Passed
```

- [x] **Step 5: Write failing test for get_product_details_by_name**

Write `tests/HealthMcp.Modules.Nutrition.Tests/McpTools/GetProductDetailsByNameToolTests.cs`:

```csharp
using HealthMcp.Modules.Nutrition.Entities;
using HealthMcp.Modules.Nutrition.Infrastructure;
using HealthMcp.Modules.Nutrition.McpTools;
using Microsoft.EntityFrameworkCore;

namespace HealthMcp.Modules.Nutrition.Tests.McpTools;

public class GetProductDetailsByNameToolTests
{
    [Fact]
    public async Task ReturnsProductDetailsWhenFound()
    {
        var options = new DbContextOptionsBuilder<NutritionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        await using var db = new NutritionDbContext(options);

        db.Products.Add(new Product
        {
            Name = "Feta cheese",
            Calories = 276,
            Protein = 16.5m,
            Fats = 23,
            Carbohydrates = 0.7m,
            Saturated = 16.1m,
            Sodium = 1116
        });
        await db.SaveChangesAsync();

        var result = await GetProductDetailsByNameTool.GetProductDetailsByName(db, "Feta cheese", CancellationToken.None);

        Assert.NotNull(result.product);
        Assert.Equal("Feta cheese", result.product.Name);
        Assert.Equal(276, result.product.Calories);
        Assert.Equal(16.1m, result.product.Saturated);
    }

    [Fact]
    public async Task ReturnsNullWhenProductNotFound()
    {
        var options = new DbContextOptionsBuilder<NutritionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        await using var db = new NutritionDbContext(options);

        var result = await GetProductDetailsByNameTool.GetProductDetailsByName(db, "Nonexistent", CancellationToken.None);

        Assert.Null(result.product);
    }
}
```

- [x] **Step 6: Run test to verify it fails**

```bash
dotnet test tests/HealthMcp.Modules.Nutrition.Tests/ --filter "FullyQualifiedName~GetProductDetailsByNameToolTests"
# Expected: Fails
```

- [x] **Step 7: Implement the tool**

Write `src/HealthMcp.Modules.Nutrition/McpTools/GetProductDetailsByNameTool.cs`:

```csharp
using System.ComponentModel;
using HealthMcp.Modules.Nutrition.Entities;
using HealthMcp.Modules.Nutrition.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;

namespace HealthMcp.Modules.Nutrition.McpTools;

[McpServerToolType]
public static class GetProductDetailsByNameTool
{
    [McpServerTool, Description("Returns complete nutritional details for a product by name")]
    public static async Task<GetProductDetailsResult> GetProductDetailsByName(
        NutritionDbContext db,
        [Description("The exact product name to look up")] string productName,
        CancellationToken cancellationToken)
    {
        var product = await db.Products
            .FirstOrDefaultAsync(p => p.Name == productName, cancellationToken);

        return new GetProductDetailsResult(product);
    }
}

public record GetProductDetailsResult(Product? product);
```

- [x] **Step 8: Run tests to verify they pass**

```bash
dotnet test tests/HealthMcp.Modules.Nutrition.Tests/ --filter "FullyQualifiedName~GetProductDetailsByNameToolTests"
# Expected: Passed
```

- [x] **Step 9: Commit**

```bash
git add -A
git commit -m "feat: add get_products and get_product_details_by_name MCP tools"
```

---

### Task 9: MCP Tools — get_daily_consumption and get_nutrient_consumption (TDD)

**Files:**
- Create: `tests/HealthMcp.Modules.Nutrition.Tests/McpTools/GetDailyConsumptionByDateRangeToolTests.cs`
- Create: `src/HealthMcp.Modules.Nutrition/McpTools/GetDailyConsumptionByDateRangeTool.cs`
- Create: `tests/HealthMcp.Modules.Nutrition.Tests/McpTools/GetNutrientConsumptionByDateRangeToolTests.cs`
- Create: `src/HealthMcp.Modules.Nutrition/McpTools/GetNutrientConsumptionByDateRangeTool.cs`

- [x] **Step 1: Write failing test for get_daily_consumption**

Write `tests/HealthMcp.Modules.Nutrition.Tests/McpTools/GetDailyConsumptionByDateRangeToolTests.cs`:

```csharp
using HealthMcp.Modules.Nutrition.Entities;
using HealthMcp.Modules.Nutrition.Infrastructure;
using HealthMcp.Modules.Nutrition.McpTools;
using Microsoft.EntityFrameworkCore;

namespace HealthMcp.Modules.Nutrition.Tests.McpTools;

public class GetDailyConsumptionByDateRangeToolTests
{
    private async Task<NutritionDbContext> SeedDatabase()
    {
        var options = new DbContextOptionsBuilder<NutritionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var db = new NutritionDbContext(options);

        var eggType = new MealType { Name = "Breakfast" };
        db.MealTypes.Add(eggType);
        await db.SaveChangesAsync();

        var eggs = new Product { Name = "Eggs", Calories = 155, Protein = 13, Fats = 11, Carbohydrates = 1.1m };
        db.Products.Add(eggs);
        await db.SaveChangesAsync();

        var meal = new Meal { Date = new DateOnly(2026, 2, 1), MealTypeId = eggType.Id };
        db.Meals.Add(meal);
        await db.SaveChangesAsync();

        db.ConsumedProducts.Add(new ConsumedProduct { MealId = meal.Id, ProductId = eggs.Id, QuantityGrams = 60 });
        await db.SaveChangesAsync();

        return db;
    }

    [Fact]
    public async Task ReturnsAggregatedDailyTotals()
    {
        await using var db = await SeedDatabase();

        var result = await GetDailyConsumptionByDateRangeTool.GetDailyConsumptionByDateRange(
            db, "2026-02-01", "2026-02-01", CancellationToken.None);

        var day = Assert.Single(result.days);
        Assert.Equal("2026-02-01", day.date);
        // 60g of eggs * (155 kcal / 100g) = 93 kcal
        Assert.Equal(93.00m, day.calories);
        // 60g * 13g protein / 100 = 7.8
        Assert.Equal(7.80m, day.protein);
    }
}
```

- [x] **Step 2: Run test to verify it fails**

```bash
dotnet test tests/HealthMcp.Modules.Nutrition.Tests/ --filter "FullyQualifiedName~GetDailyConsumptionByDateRangeToolTests"
```

- [x] **Step 3: Implement the tool**

Write `src/HealthMcp.Modules.Nutrition/McpTools/GetDailyConsumptionByDateRangeTool.cs`:

```csharp
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

        var consumed = await db.ConsumedProducts
            .Where(c => c.Meal.Date >= from && c.Meal.Date <= to)
            .Select(c => new
            {
                c.Meal.Date,
                c.QuantityGrams,
                c.Product.Calories,
                c.Product.Protein,
                c.Product.Fats,
                c.Product.Carbohydrates
            })
            .ToListAsync(cancellationToken);

        var days = consumed
            .GroupBy(c => c.Date)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var factor = 1m / 100m;
                return new DaySummary(
                    g.Key.ToString("yyyy-MM-dd"),
                    Math.Round(g.Sum(x => x.Calories * x.QuantityGrams * factor), 2),
                    Math.Round(g.Sum(x => x.Protein * x.QuantityGrams * factor), 2),
                    Math.Round(g.Sum(x => x.Fats * x.QuantityGrams * factor), 2),
                    Math.Round(g.Sum(x => x.Carbohydrates * x.QuantityGrams * factor), 2)
                );
            })
            .ToList();

        return new DailyConsumptionResult(days);
    }
}

public record DaySummary(string date, decimal calories, decimal protein, decimal fats, decimal carbs);
public record DailyConsumptionResult(List<DaySummary> days);
```

- [x] **Step 4: Run tests to verify they pass**

```bash
dotnet test tests/HealthMcp.Modules.Nutrition.Tests/ --filter "FullyQualifiedName~GetDailyConsumptionByDateRangeToolTests"
```

- [x] **Step 5: Write failing test for get_nutrient_consumption**

Write `tests/HealthMcp.Modules.Nutrition.Tests/McpTools/GetNutrientConsumptionByDateRangeToolTests.cs`:

```csharp
using HealthMcp.Modules.Nutrition.Entities;
using HealthMcp.Modules.Nutrition.Infrastructure;
using HealthMcp.Modules.Nutrition.McpTools;
using Microsoft.EntityFrameworkCore;

namespace HealthMcp.Modules.Nutrition.Tests.McpTools;

public class GetNutrientConsumptionByDateRangeToolTests
{
    private async Task<NutritionDbContext> SeedDatabase()
    {
        var options = new DbContextOptionsBuilder<NutritionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var db = new NutritionDbContext(options);

        var mt = new MealType { Name = "Breakfast" };
        db.MealTypes.Add(mt);
        await db.SaveChangesAsync();

        var eggs = new Product { Name = "Eggs", Calories = 155, Protein = 13, Fats = 11, Carbohydrates = 1.1m, Saturated = 3.2m };
        var cheese = new Product { Name = "Feta cheese", Calories = 276, Protein = 16.5m, Fats = 23, Carbohydrates = 0.7m, Saturated = 16.1m };
        db.Products.AddRange(eggs, cheese);
        await db.SaveChangesAsync();

        var meal = new Meal { Date = new DateOnly(2026, 2, 1), MealTypeId = mt.Id };
        db.Meals.Add(meal);
        await db.SaveChangesAsync();

        db.ConsumedProducts.AddRange(
            new ConsumedProduct { MealId = meal.Id, ProductId = eggs.Id, QuantityGrams = 60 },
            new ConsumedProduct { MealId = meal.Id, ProductId = cheese.Id, QuantityGrams = 30 }
        );
        await db.SaveChangesAsync();

        return db;
    }

    [Fact]
    public async Task AggregatedMode_ReturnsTotalPerDay()
    {
        await using var db = await SeedDatabase();

        var result = await GetNutrientConsumptionByDateRangeTool.GetNutrientConsumptionByDateRange(
            db, "2026-02-01", "2026-02-01", "saturated", "aggregated", CancellationToken.None);

        var day = Assert.Single(result.days);
        // 60g eggs * 3.2/100 + 30g cheese * 16.1/100 = 1.92 + 4.83 = 6.75
        Assert.Equal(6.75m, day.value);
    }

    [Fact]
    public async Task DetailedMode_ReturnsBreakdownByProduct()
    {
        await using var db = await SeedDatabase();

        var result = await GetNutrientConsumptionByDateRangeTool.GetNutrientConsumptionByDateRange(
            db, "2026-02-01", "2026-02-01", "saturated", "detailed", CancellationToken.None);

        var day = Assert.Single(result.days);
        Assert.Equal(2, day.products!.Count);
        Assert.Contains(day.products!, p => p.name == "Eggs" && p.value == 1.92m);
        Assert.Contains(day.products!, p => p.name == "Feta cheese" && p.value == 4.83m);
    }

    [Fact]
    public async Task UnknownNutrient_ReturnsError()
    {
        await using var db = await SeedDatabase();

        var result = await GetNutrientConsumptionByDateRangeTool.GetNutrientConsumptionByDateRange(
            db, "2026-02-01", "2026-02-01", "unknown", "aggregated", CancellationToken.None);

        Assert.True(result.IsError);
    }
}
```

- [x] **Step 6: Run test to verify it fails**

```bash
dotnet test tests/HealthMcp.Modules.Nutrition.Tests/ --filter "FullyQualifiedName~GetNutrientConsumptionByDateRangeToolTests"
```

- [x] **Step 7: Implement the tool**

Write `src/HealthMcp.Modules.Nutrition/McpTools/GetNutrientConsumptionByDateRangeTool.cs`:

```csharp
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
                Product = c.Product // needed for in-memory nutrient selector
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
```

- [x] **Step 8: Run tests to verify they pass**

```bash
dotnet test tests/HealthMcp.Modules.Nutrition.Tests/ --filter "FullyQualifiedName~GetNutrientConsumptionByDateRangeToolTests"
# Expected: Passed
```

- [x] **Step 9: Commit**

```bash
git add -A
git commit -m "feat: add get_daily_consumption and get_nutrient_consumption MCP tools"
```

---

### Task 10: MCP Tools — get_frequency and get_meal_detail (TDD)

**Files:**
- Create: `tests/HealthMcp.Modules.Nutrition.Tests/McpTools/GetFrequencyWithinDateRangeByProductsToolTests.cs`
- Create: `src/HealthMcp.Modules.Nutrition/McpTools/GetFrequencyWithinDateRangeByProductsTool.cs`
- Create: `tests/HealthMcp.Modules.Nutrition.Tests/McpTools/GetMealDetailByDateToolTests.cs`
- Create: `src/HealthMcp.Modules.Nutrition/McpTools/GetMealDetailByDateTool.cs`

- [ ] **Step 1: Write failing test for get_frequency**

Write `tests/HealthMcp.Modules.Nutrition.Tests/McpTools/GetFrequencyWithinDateRangeByProductsToolTests.cs`:

```csharp
using HealthMcp.Modules.Nutrition.Entities;
using HealthMcp.Modules.Nutrition.Infrastructure;
using HealthMcp.Modules.Nutrition.McpTools;
using Microsoft.EntityFrameworkCore;

namespace HealthMcp.Modules.Nutrition.Tests.McpTools;

public class GetFrequencyWithinDateRangeByProductsToolTests
{
    [Fact]
    public async Task ReturnsProductFrequencies()
    {
        var options = new DbContextOptionsBuilder<NutritionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        await using var db = new NutritionDbContext(options);

        var mt = new MealType { Name = "Breakfast" };
        db.MealTypes.Add(mt);
        await db.SaveChangesAsync();

        var eggs = new Product { Name = "Eggs", Calories = 155, Protein = 13, Fats = 11, Carbohydrates = 1.1m };
        var cheese = new Product { Name = "Feta cheese", Calories = 276, Protein = 16.5m, Fats = 23, Carbohydrates = 0.7m };
        db.Products.AddRange(eggs, cheese);
        await db.SaveChangesAsync();

        var meal1 = new Meal { Date = new DateOnly(2026, 2, 1), MealTypeId = mt.Id };
        var meal2 = new Meal { Date = new DateOnly(2026, 2, 2), MealTypeId = mt.Id };
        db.Meals.AddRange(meal1, meal2);
        await db.SaveChangesAsync();

        db.ConsumedProducts.AddRange(
            new ConsumedProduct { MealId = meal1.Id, ProductId = eggs.Id, QuantityGrams = 60 },
            new ConsumedProduct { MealId = meal2.Id, ProductId = eggs.Id, QuantityGrams = 60 },
            new ConsumedProduct { MealId = meal1.Id, ProductId = cheese.Id, QuantityGrams = 30 }
        );
        await db.SaveChangesAsync();

        var result = await GetFrequencyWithinDateRangeByProductsTool.GetFrequencyWithinDateRangeByProducts(
            db, "2026-02-01", "2026-02-02", CancellationToken.None);

        Assert.Equal(2, result.products.Count);
        var eggsFreq = result.products.First(p => p.name == "Eggs");
        Assert.Equal(2, eggsFreq.count);
        var cheeseFreq = result.products.First(p => p.name == "Feta cheese");
        Assert.Equal(1, cheeseFreq.count);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

```bash
dotnet test tests/HealthMcp.Modules.Nutrition.Tests/ --filter "FullyQualifiedName~GetFrequencyWithinDateRangeByProductsToolTests"
```

- [ ] **Step 3: Implement the tool**

Write `src/HealthMcp.Modules.Nutrition/McpTools/GetFrequencyWithinDateRangeByProductsTool.cs`:

```csharp
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

        var frequencies = await db.ConsumedProducts
            .Where(c => c.Meal.Date >= from && c.Meal.Date <= to)
            .GroupBy(c => c.Product.Name)
            .Select(g => new ProductFrequency(g.Key, g.Count()))
            .OrderByDescending(f => f.count)
            .ToListAsync(cancellationToken);

        return new FrequencyResult(frequencies);
    }
}

public record ProductFrequency(string name, int count);
public record FrequencyResult(List<ProductFrequency> products);
```

- [ ] **Step 4: Run tests to verify they pass**

```bash
dotnet test tests/HealthMcp.Modules.Nutrition.Tests/ --filter "FullyQualifiedName~GetFrequencyWithinDateRangeByProductsToolTests"
```

- [ ] **Step 5: Write failing test for get_meal_detail**

Write `tests/HealthMcp.Modules.Nutrition.Tests/McpTools/GetMealDetailByDateToolTests.cs`:

```csharp
using HealthMcp.Modules.Nutrition.Entities;
using HealthMcp.Modules.Nutrition.Infrastructure;
using HealthMcp.Modules.Nutrition.McpTools;
using Microsoft.EntityFrameworkCore;

namespace HealthMcp.Modules.Nutrition.Tests.McpTools;

public class GetMealDetailByDateToolTests
{
    [Fact]
    public async Task ReturnsMealBreakdownForDate()
    {
        var options = new DbContextOptionsBuilder<NutritionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        await using var db = new NutritionDbContext(options);

        var breakfast = new MealType { Name = "Breakfast" };
        var lunch = new MealType { Name = "Lunch" };
        db.MealTypes.AddRange(breakfast, lunch);
        await db.SaveChangesAsync();

        var eggs = new Product { Name = "Eggs", Calories = 155, Protein = 13, Fats = 11, Carbohydrates = 1.1m };
        db.Products.Add(eggs);
        await db.SaveChangesAsync();

        var meal = new Meal { Date = new DateOnly(2026, 2, 1), MealTypeId = breakfast.Id };
        db.Meals.Add(meal);
        await db.SaveChangesAsync();

        db.ConsumedProducts.Add(new ConsumedProduct { MealId = meal.Id, ProductId = eggs.Id, QuantityGrams = 60 });
        await db.SaveChangesAsync();

        var result = await GetMealDetailByDateTool.GetMealDetailByDate(
            db, "2026-02-01", CancellationToken.None);

        var m = Assert.Single(result.meals);
        Assert.Equal("Breakfast", m.mealType);
        var p = Assert.Single(m.products);
        Assert.Equal("Eggs", p.name);
        Assert.Equal(60, p.quantityGrams);
        Assert.Equal(93.00m, p.calories); // 60 * 155 / 100
    }

    [Fact]
    public async Task ReturnsEmptyWhenNoDataForDate()
    {
        var options = new DbContextOptionsBuilder<NutritionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        await using var db = new NutritionDbContext(options);

        var result = await GetMealDetailByDateTool.GetMealDetailByDate(
            db, "2099-01-01", CancellationToken.None);

        Assert.Empty(result.meals);
    }
}
```

- [ ] **Step 6: Run test to verify it fails**

```bash
dotnet test tests/HealthMcp.Modules.Nutrition.Tests/ --filter "FullyQualifiedName~GetMealDetailByDateToolTests"
```

- [ ] **Step 7: Implement the tool**

Write `src/HealthMcp.Modules.Nutrition/McpTools/GetMealDetailByDateTool.cs`:

```csharp
using System.ComponentModel;
using HealthMcp.Modules.Nutrition.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;

namespace HealthMcp.Modules.Nutrition.McpTools;

[McpServerToolType]
public static class GetMealDetailByDateTool
{
    [McpServerTool, Description("Returns full meal breakdown including all products and their nutritional values for a specific date")]
    public static async Task<MealDetailResult> GetMealDetailByDate(
        NutritionDbContext db,
        [Description("Date (YYYY-MM-DD)")] string date,
        CancellationToken cancellationToken)
    {
        var d = DateOnly.Parse(date);

        var meals = await db.Meals
            .Include(m => m.MealType)
            .Include(m => m.ConsumedProducts)
                .ThenInclude(cp => cp.Product)
            .Where(m => m.Date == d)
            .OrderBy(m => m.MealType.Name)
            .ToListAsync(cancellationToken);

        var mealDetails = meals.Select(m => new MealDetail(
            m.MealType.Name,
            m.ConsumedProducts.Select(cp =>
            {
                var factor = cp.QuantityGrams / 100m;
                return new ProductConsumedDetail(
                    cp.Product.Name,
                    cp.QuantityGrams,
                    Math.Round(cp.Product.Calories * factor, 2)
                );
            }).ToList()
        )).ToList();

        return new MealDetailResult(mealDetails);
    }
}

public record ProductConsumedDetail(string name, decimal quantityGrams, decimal calories);
public record MealDetail(string mealType, List<ProductConsumedDetail> products);
public record MealDetailResult(List<MealDetail> meals);
```

- [ ] **Step 8: Run tests to verify they pass**

```bash
dotnet test tests/HealthMcp.Modules.Nutrition.Tests/ --filter "FullyQualifiedName~GetMealDetailByDateToolTests"
```

- [ ] **Step 9: Run all tests**

```bash
dotnet test tests/HealthMcp.Modules.Nutrition.Tests/
# Expected: All tests pass
```

- [ ] **Step 10: Commit**

```bash
git add -A
git commit -m "feat: add get_frequency and get_meal_detail MCP tools"
```

---

### Task 11: MCP Server Host

**Files:**
- Modify: `src/HealthMcp.McpServer/Program.cs`
- Create: `src/HealthMcp.McpServer/appsettings.json`

- [ ] **Step 1: Write Program.cs**

Write `src/HealthMcp.McpServer/Program.cs`:

```csharp
using HealthMcp.Modules.Nutrition.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<NutritionDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddMcpServer()
    .WithHttpTransport(options =>
    {
        options.Stateless = true;
    })
    .WithToolsFromAssembly(typeof(NutritionDbContext).Assembly);

var app = builder.Build();

app.MapMcp();

app.Run();
```

- [ ] **Step 2: Create appsettings.json**

Write `src/HealthMcp.McpServer/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=health_mcp;Username=postgres;Password=postgres"
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:9093"
      }
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

- [ ] **Step 3: Build and verify**

```bash
dotnet build src/HealthMcp.McpServer/
# Expected: Build succeeded
```

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "feat: add MCP Server host on port 9093"
```

---

### Task 12: Integration Test — End-to-End

**Files:**
- Create: `tests/HealthMcp.Api.Tests/HealthMcpApiFactory.cs`

- [ ] **Step 1: Create test factory that uses in-memory database**

Write `tests/HealthMcp.Api.Tests/HealthMcpApiFactory.cs`:

```csharp
using HealthMcp.Modules.Nutrition.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

namespace HealthMcp.Api.Tests;

public class HealthMcpApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<NutritionDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<NutritionDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));
        });
    }
}
```

- [ ] **Step 2: Update integration test to use factory**

Update `tests/HealthMcp.Api.Tests/CsvImportEndpointTests.cs`:

```csharp
using System.Net;
using System.Text;
using System.Text.Json;

namespace HealthMcp.Api.Tests;

public class CsvImportEndpointTests : IClassFixture<HealthMcpApiFactory>
{
    private readonly HealthMcpApiFactory _factory;

    public CsvImportEndpointTests(HealthMcpApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PostCsvImport_ReturnsOkWithSummary()
    {
        var client = _factory.CreateClient();

        var csv = "Date,Meal,\"Products and dishes\",\"quantity (g)\",\"calories (kcal)\",\"Protein (g)\",\"Fats (g)\",\"Carbohydrates (g)\"\n"
                + "2026-02-01,Breakfast,\"Feta cheese\",6,16.56,0.99,1.38,0.042";

        var content = new StringContent(csv, Encoding.UTF8, "text/csv");
        var response = await client.PostAsync("/api/import/csv", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(body);
        Assert.Equal(1, result.GetProperty("productsImported").GetInt32());
    }
}
```

- [ ] **Step 3: Run the integration test**

```bash
dotnet test tests/HealthMcp.Api.Tests/
# Expected: Passed
```

- [ ] **Step 4: Final build check**

```bash
dotnet build
# Expected: Build succeeded, 0 warnings
```

- [ ] **Step 5: Run all tests**

```bash
dotnet test
# Expected: All tests pass
```

- [ ] **Step 6: Commit**

```bash
git add -A
git commit -m "test: add integration tests and test factory"
```

---

### Verification

After completing all tasks, verify the full system:

```bash
# 1. Run all tests
dotnet test

# 2. Start PostgreSQL if not running
docker compose up -d

# 3. Apply any pending migrations
dotnet ef database update --project src/HealthMcp.Modules.Nutrition --startup-project src/HealthMcp.Api

# 4. Start API in background
dotnet run --project src/HealthMcp.Api &
API_PID=$!
sleep 3

# 5. Import test CSV
curl -X POST -H "Content-Type: text/csv" --data-binary \
"Date,Meal,\"Products and dishes\",\"quantity (g)\",\"calories (kcal)\",\"Protein (g)\",\"Fats (g)\",\"Carbohydrates (g)\"
2026-02-01,Breakfast,\"Feta cheese\",6,16.56,0.99,1.38,0.042
2026-02-01,Breakfast,\"Polskie jaja z chowu ściółkowego M\",60,83.4,7.5,5.82,0.36
2026-02-01,Breakfast,\"Gorąco polecam Bułka brioshe\",15,44.85,1.245,0.9,7.8" \
http://localhost:9092/api/import/csv

# 6. Kill API
kill $API_PID

# 7. Start MCP Server in background
dotnet run --project src/HealthMcp.McpServer &
MCP_PID=$!
sleep 3

# 8. Test MCP tools via curl (Streamable HTTP)
curl -X POST -H "Content-Type: application/json" -d '{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "tools/call",
  "params": {
    "name": "get_products",
    "arguments": {}
  }
}' http://localhost:9093/mcp

# 9. Kill MCP Server
kill $MCP_PID
```
