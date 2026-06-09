# Fuzzy Product Search — Future Enhancement

**Status:** Future — not yet planned for implementation.
**Date:** 2026-06-09

## Summary

Extend the `get_product_details_by_name` MCP tool to support fuzzy name matching using PostgreSQL trigram similarity, so that searching `"rice"` returns all products containing `"rice"` in their name (e.g., "Brown Rice", "Rice Crackers"), and typo-tolerant searches like `"rie"` return likely matches with similarity scores.

## Motivation

The current tool performs exact name matching (`p.Name == productName`). If even one character is wrong, the search fails to find the product. This enhancement adds a `matchMode` parameter to support both exact and fuzzy matching in a single tool.

## Design

### Modified Tool Signature

```
get_product_details_by_name(
  productName: string,
  matchMode: "exact" | "fuzzy"   // default: "exact"
)
```

Default is `"exact"` — backward compatible with all existing callers.

### Return Type

```csharp
public record ProductSearchResult(
    Product? Product,                      // populated in exact mode
    IReadOnlyList<ProductMatch>? Matches   // populated in fuzzy mode
);

public record ProductMatch(
    Product Product,
    double Similarity                      // 0.0 to 1.0 (pg_trgm score)
);
```

### Behavior by Mode

| Mode | Query | Result |
|---|---|---|
| `exact` | `p.Name == productName` (current) | Single `Product` or null |
| `fuzzy` | `SELECT *, similarity(name, @term) AS sim FROM products WHERE similarity(name, @term) > 0.3 ORDER BY sim DESC LIMIT 10` | Up to 10 `ProductMatch` entries, sorted by similarity descending |

### Infrastructure

- PostgreSQL `pg_trgm` extension must be enabled.
- EF Core uses raw SQL for the fuzzy query since it cannot express trigram functions natively.
- Similarity threshold: `0.3` (pg_trgm default, adjustable via configuration).
- Result limit: `10` (configurable).

### Migration

```sql
CREATE EXTENSION IF NOT EXISTS pg_trgm;
```

Added via an EF Core migration.

### Implementation Plan (when ready)

1. **Migration**: Add `CREATE EXTENSION pg_trgm` migration.
2. **Return type**: Replace `GetProductDetailsResult` with `ProductSearchResult` + `ProductMatch` records.
3. **Tool update**: Add `matchMode` parameter with default `"exact"` to `GetProductDetailsByNameTool`.
4. **Fuzzy query**: Add raw SQL branch using `similarity()` function.
5. **Tests**:
   - Exact mode: existing tests continue to pass unchanged.
   - Fuzzy mode: integration test with real PostgreSQL (Testcontainers) — seed "Brown Rice", "Wild Rice", "Rice Crackers"; search `"rice"` returns all 3 with scores > 0.3; search `"rie"` returns them with slightly lower scores.

## Backward Compatibility

- Default `matchMode = "exact"` ensures no breakage.
- Existing consumers see no behavioral change unless they opt into `"fuzzy"`.

## Open Questions

- Should similarity threshold be exposed as a third parameter? Deferred — start with a configurable constant.
