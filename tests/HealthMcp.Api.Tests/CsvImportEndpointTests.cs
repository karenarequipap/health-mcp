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
