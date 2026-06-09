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
