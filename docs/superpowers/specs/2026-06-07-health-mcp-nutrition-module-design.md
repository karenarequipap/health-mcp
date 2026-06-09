# Health MCP — Nutrition Module Design

## Overview

A .NET 10 solution implementing a modular Model Context Protocol (MCP) server for health tracking. This spec covers Module 1 (Nutrition): CSV import from Fitatu, product/meal tracking, and MCP query tools. The API and MCP server run as separate processes sharing a common database and module library.

## Architecture

- **Solution layout**: Modular monolith — each domain (Nutrition, future Sleep/Anthropometric/Activity) is a self-contained project with its own entities, EF configuration, services, and MCP tools.
- **API** (ASP.NET Core Minimal API, port `9092`): single CSV import endpoint.
- **MCP Server** (port `9093`): hosts MCP tools discovered from registered modules.
- **Shared database**: PostgreSQL via EF Core, shared across API and MCP Server.
- **Technology stack**: .NET 10 LTS, ModelContextProtocol SDK (official C# SDK), EF Core 10, Npgsql.

## Solution Structure

```
HealthMcp.sln
├── src/
│   ├── HealthMcp.Modules.Nutrition/      — Domain, EF config, MCP tools, CSV parsing
│   ├── HealthMcp.Api/                    — ASP.NET Core Minimal API host (port 9092)
│   └── HealthMcp.McpServer/              — MCP host (port 9093), discovers tools from modules
└── docs/
    └── superpowers/
        └── specs/
            └── 2026-06-07-health-mcp-nutrition-module-design.md
```

### Project Dependencies

```
HealthMcp.Api ──→ HealthMcp.Modules.Nutrition
HealthMcp.McpServer ──→ HealthMcp.Modules.Nutrition
```

Both API and MCP Server host projects set up DI, DbContext, and routing in `Program.cs`. The Nutrition module provides services and tools consumed by both.

## Nutrition Module

### Entities

#### Product

Stores nutritional data **per 100g** of product. Maps directly from CSV columns.

| Column | Type | Notes |
|---|---|---|---|
| `Id` | `int` (PK, auto-increment) | |
| `Name` | `string` (required) | Product name from CSV "Products and dishes" column |
| `Calories` | `decimal` (required) | kcal per 100g |
| `Protein` | `decimal` (required) | g per 100g |
| `Fats` | `decimal` (required) | g per 100g |
| `Carbohydrates` | `decimal` (required) | g per 100g |
| `PlantProtein` | `decimal?` | g per 100g |
| `AnimalProtein` | `decimal?` | g per 100g |
| `Saturated` | `decimal?` | g per 100g |
| `Monounsaturated` | `decimal?` | g per 100g |
| `Polyunsaturated` | `decimal?` | g per 100g |
| `Omega3` | `decimal?` | g per 100g |
| `Omega6` | `decimal?` | g per 100g |
| `Sugars` | `decimal?` | g per 100g |
| `Cholesterol` | `decimal?` | mg per 100g |
| `Fibre` | `decimal?` | g per 100g |
| `Caffeine` | `decimal?` | mg per 100g |
| `FolicAcid` | `decimal?` | ug per 100g |
| `VitaminA` | `decimal?` | ug per 100g |
| `VitaminB1` | `decimal?` | mg per 100g |
| `VitaminB2` | `decimal?` | mg per 100g |
| `VitaminB5` | `decimal?` | mg per 100g |
| `VitaminB6` | `decimal?` | mg per 100g |
| `Biotin` | `decimal?` | ug per 100g |
| `VitaminB12` | `decimal?` | ug per 100g |
| `VitaminC` | `decimal?` | mg per 100g |
| `VitaminD` | `decimal?` | ug per 100g |
| `VitaminE` | `decimal?` | mg per 100g |
| `VitaminPP` | `decimal?` | mg per 100g |
| `VitaminK` | `decimal?` | ug per 100g |
| `Zinc` | `decimal?` | mg per 100g |
| `Phosphorous` | `decimal?` | mg per 100g |
| `Iodine` | `decimal?` | ug per 100g |
| `Magnesium` | `decimal?` | mg per 100g |
| `Copper` | `decimal?` | mg per 100g |
| `Potassium` | `decimal?` | mg per 100g |
| `Selenium` | `decimal?` | ug per 100g |
| `Sodium` | `decimal?` | mg per 100g |
| `Calcium` | `decimal?` | mg per 100g |
| `Iron` | `decimal?` | mg per 100g |
| `Salt` | `decimal?` | g per 100g |

**Required fields**: Name, Calories, Protein, Fats, Carbohydrates. All other nutritional columns are optional (nullable, no default value).

**Unique constraint**: `(Name, Calories)` — same product name with different calorie count is treated as a different product.

#### MealType

Stores meal names from the CSV "Meal" column.

| Column | Type | Notes |
|---|---|---|
| `Id` | `int` (PK, auto-increment) | |
| `Name` | `string` (required, unique) | e.g., "Breakfast", "Snack I", "Lunch" |

#### Meal

A specific meal instance linking a date to a meal type.

| Column | Type | Notes |
|---|---|---|
| `Id` | `int` (PK, auto-increment) | |
| `Date` | `DateOnly` (required) | |
| `MealTypeId` | `int` (FK → MealType) | |

**Unique constraint**: `(Date, MealTypeId)`.

#### ConsumedProduct

Links a product consumed in a meal with its quantity.

| Column | Type | Notes |
|---|---|---|
| `Id` | `int` (PK, auto-increment) | |
| `MealId` | `int` (FK → Meal) | |
| `ProductId` | `int` (FK → Product) | |
| `QuantityGrams` | `decimal` | Quantity consumed in grams |

**Unique constraint**: `(MealId, ProductId, QuantityGrams)` — prevents duplicate entries on re-import.

### Database Schema

```sql
CREATE TABLE MealTypes (
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE Products (
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(500) NOT NULL,
    Calories DECIMAL NOT NULL,
    Protein DECIMAL NOT NULL,
    Fats DECIMAL NOT NULL,
    Carbohydrates DECIMAL NOT NULL,
    PlantProtein DECIMAL NULL,
    AnimalProtein DECIMAL NULL,
    ... (remaining optional nutritional columns, all NULL)
    UNIQUE (Name, Calories)
);

CREATE TABLE Meals (
    Id SERIAL PRIMARY KEY,
    Date DATE NOT NULL,
    MealTypeId INT NOT NULL REFERENCES MealTypes(Id),
    UNIQUE (Date, MealTypeId)
);

CREATE TABLE ConsumedProducts (
    Id SERIAL PRIMARY KEY,
    MealId INT NOT NULL REFERENCES Meals(Id),
    ProductId INT NOT NULL REFERENCES Products(Id),
    QuantityGrams DECIMAL NOT NULL,
    UNIQUE (MealId, ProductId, QuantityGrams)
);
```

## REST API — CSV Import

### Endpoint

```
POST /api/import/csv
Content-Type: text/csv
Body: <raw CSV content>
```

### Response

```json
{
  "productsImported": 10,
  "productsSkipped": 32,
  "mealsCreated": 3,
  "consumedProductsCreated": 8,
  "consumedProductsSkipped": 5,
  "errors": ["line 45: invalid quantity value, skipped"]
}
```

### Import Logic (CsvImportService)

1. Parse CSV headers to validate and map column positions
2. For each data row:
   - Extract product name (normalize whitespace)
   - Extract all nutritional columns (parse decimals, handle empty as null)
   - Check Product table for `(Name, Calories)` match → upsert or reuse
   - Check MealType table for `(Name)` match → create if new
   - Check Meal table for `(Date, MealTypeId)` match → create if new
   - Check ConsumedProduct table for `(MealId, ProductId, QuantityGrams)` match → skip if exists
3. Collect per-row errors as strings (invalid numbers, missing required fields)
4. Return summary with counts (successes and skips broken out)

## MCP Tools

### Tool Definitions

| Tool | Description | Input | Output |
|---|---|---|---|
| `get_products` | List all products | — | `{ products: [{ name, energy, protein, fats, carbs }] }` |
| `get_product_details_by_name` | Full nutritional info | `{ productName: string }` | `{ product: { ...all columns } }` |
| `get_daily_consumption_by_date_range` | Aggregated daily totals | `{ startDate, endDate }` | `{ days: [{ date, calories, protein, fat, carbs }] }` |
| `get_nutrient_consumption_by_date_range` | Consumption of a specific nutrient in date range | `{ startDate, endDate, nutrient: string, mode: "aggregated"|"detailed" }` | Aggregated: `{ days: [{ date, value }] }` — Detailed: `{ days: [{ date, products: [{ name, value }] }] }` |
| `get_frequency_within_date_range_by_products` | Consumption frequency per product | `{ startDate, endDate }` | `{ products: [{ name, count }] }` |
| `get_meal_detail_by_date` | Full meal breakdown for a date | `{ date }` | `{ meals: [{ mealType, products: [{ name, quantityGrams, calories }] }] }` |

### Nutritional Calculation

For `get_daily_consumption_by_date_range` and `get_meal_detail_by_date`, values are computed as:

```
nutrient_consumed = product_nutrient_per_100g × quantity_grams / 100
```

## Ports

- **API**: `9092` (configured via `appsettings.json`)
- **MCP Server**: `9093` (configured via `appsettings.json`)

## Error Handling

- CSV import: per-row errors are collected, non-fatal rows don't block the batch
- MCP tools: return structured error content for invalid inputs (missing products, bad date formats)
- Database: EF Core handles connection/query errors, propagated up

## Testing

- Unit tests: `CsvImportService` parsing logic, duplicate detection
- Integration tests: CSV import endpoint (in-memory or test database)
- Integration tests: MCP tool queries against test data

## Future Modules

Designed for extension — each future module (Sleep, Anthropometric, Activity) follows the same pattern:
- New project `HealthMcp.Modules.{Name}`
- Its own entities, EF config, MCP tools
- Registered in API and MCP Server `Program.cs`
