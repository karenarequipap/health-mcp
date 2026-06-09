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

public partial class Program { }
