# Health MCP — Agent Guide

Compact reference for AI agents working in this repo.

## Project

- **Goal**: MCP server + REST API for ingesting and querying nutrition data from CSV exports.
- **Stack**: .NET 10 LTS, EF Core 10 + Npgsql, ModelContextProtocol.AspNetCore SDK, PostgreSQL 16, xUnit + Moq, CsvHelper.
- **Entrypoints**:
  - `src/HealthMcp.Api/` — REST API server (port 9092)
  - `src/HealthMcp.McpServer/` — MCP server (port 9093)
- **Database**: relational (PostgreSQL) with schema: Product → MealType → Meal → ConsumedProduct.

## Commands

| Task | Command |
|------|---------|
| Build solution | `dotnet build` |
| Run all tests | `dotnet test` |
| Run specific tests | `dotnet test --filter "FullyQualifiedName~<TestClass>"` |
| Add migration | `dotnet ef migrations add <Name> --project src/HealthMcp.Modules.Nutrition --startup-project src/HealthMcp.Api` |
| Apply migrations | `dotnet ef database update --project src/HealthMcp.Modules.Nutrition --startup-project src/HealthMcp.Api` |
| Run API | `dotnet run --project src/HealthMcp.Api` |
| Run MCP Server | `dotnet run --project src/HealthMcp.McpServer` |
| Import CSV | `curl -X POST -H "Content-Type: text/csv" --data-binary @file.csv http://localhost:9092/api/import/csv` |
| Start PostgreSQL | `docker compose up -d` |
| Check DB tables | `docker compose exec postgres psql -U postgres -d health_mcp -c "\dt"` |

## State of the Codebase

- **Current state**: Solution scaffolding is complete (projects, references, packages, Docker, solution file). No implementation code written yet — remaining work includes domain entities (Task 3), EF configurations + DbContext (4), initial migration (5), CSV import service TDD (6), CSV import API endpoint (7), MCP tools TDD (8-10), MCP Server host (11), and integration tests (12). Full plan at `docs/superpowers/plans/2026-06-07-nutrition-module.md`.
- Domain model: `Product` (nutritional info per 100g), `MealType` (Breakfast/Lunch/etc.), `Meal` (date + meal type), `ConsumedProduct` (product + quantity in grams).
- All MCP tools use EF Core `NutritionDbContext` with PostgreSQL and are registered via `[McpServerToolType]` + `[McpServerTool]`.

## Local Development

1. Start PostgreSQL:

```bash
docker compose up -d
```

2. Build the solution:

```bash
dotnet build
```

3. Apply database migrations:

```bash
dotnet ef database update --project src/HealthMcp.Modules.Nutrition --startup-project src/HealthMcp.Api
```

4. Run tests:

```bash
dotnet test
```

## Architecture (from README)

- **REST API**: `POST /api/import/csv` to ingest nutrition CSV exports.
- **MCP Tools**: `get_products`, `get_product_details_by_name`, `get_daily_consumption`, `get_nutrient_consumption`, `get_frequency`, `get_meal_detail`.
- **Test Approach**: TDD — tests precede implementation (xUnit + Moq + EF Core InMemory).
- **DB Schema**: Tables `Products`, `MealTypes`, `Meals`, `ConsumedProducts` — see `README.md` for ER diagram. Products have a unique index on `(Name, Calories)`. ConsumedProducts have a unique index on `(MealId, ProductId, QuantityGrams)` for duplicate row detection.

## Progress Tracking

Progress against the implementation plan is tracked in `docs/superpowers/plans/2026-06-07-nutrition-module.md` using Markdown checkboxes (`- [ ]` / `- [x]`).

**Rule:** After completing any task from the plan, update the plan file by changing `- [ ]` to `- [x]` for all steps in that task. This keeps progress visible and verifiable.

## Notes for Agents

- When implementing, match the entity schema and tool signatures described in `README.md`.
- Use `dotnet build` to verify compilation after each commit-able milestone.
- Commit after each task with descriptive commit messages.
