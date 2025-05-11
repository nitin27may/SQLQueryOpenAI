using SQLQueryAI;
using SQLQueryAI.services;
using static SQLQueryAI.services.SqlAiServiceFactory;

var builder = WebApplication.CreateBuilder(args);

// 1. Add services to the container.
builder.Services.AddOpenApi(); // For minimal APIs or MVC, depending on your approach

// ADD THIS to enable controllers
builder.Services.AddControllers();

// Configure the AI service based on appsettings
var aiConfig = builder.Configuration.GetSection("AI");
var providerTypeStr = aiConfig["ProviderType"];

if (!Enum.TryParse<ProviderType>(providerTypeStr, out var providerType))
{
    providerType = ProviderType.OpenAI; // Default to OpenAI if not specified or invalid
}

// Register the AI service factory and configuration
builder.Services.AddSingleton<ISqlAiService>(sp =>
{
    // Create the appropriate configuration based on provider type
    var aiServiceConfig = new AiServiceConfiguration
    {
        ApiKey = providerType switch
        {
            ProviderType.OpenAI => aiConfig["OpenAI:ApiKey"],
            ProviderType.AzureOpenAI => aiConfig["AzureOpenAI:ApiKey"],
            ProviderType.Claude => aiConfig["Claude:ApiKey"],
            ProviderType.Gemini => aiConfig["Gemini:ApiKey"],
            _ => aiConfig["OpenAI:ApiKey"]
        },
        Endpoint = aiConfig["AzureOpenAI:Endpoint"],
        DeploymentName = aiConfig["AzureOpenAI:DeploymentName"],
        Model = providerType switch
        {
            ProviderType.OpenAI => aiConfig["OpenAI:Model"],
            ProviderType.AzureOpenAI => aiConfig["AzureOpenAI:Model"],
            ProviderType.Claude => aiConfig["Claude:Model"],
            ProviderType.Gemini => aiConfig["Gemini:Model"],
            _ => aiConfig["OpenAI:Model"]
        }
    };

    // Get the logger for the appropriate service type
    ILogger logger = providerType switch
    {
        ProviderType.OpenAI => sp.GetRequiredService<ILogger<SQLQueryAI.services.OpenAiService>>(),
        ProviderType.AzureOpenAI => sp.GetRequiredService<ILogger<AzureOpenAiService>>(),
        ProviderType.Claude => sp.GetRequiredService<ILogger<ClaudeService>>(),
        ProviderType.Gemini => sp.GetRequiredService<ILogger<GeminiService>>(),
        _ => sp.GetRequiredService<ILogger<SQLQueryAI.services.OpenAiService>>()
    };

    // Create the service using the factory
    return SqlAiServiceFactory.CreateService(providerType, aiServiceConfig, logger);
});

// Database connection string
string connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"] ?? "";
builder.Services.AddSingleton<DatabaseService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<DatabaseService>>();
    return new DatabaseService(connectionString, logger);
});

var app = builder.Build();

// 2. Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// ADD THIS to map all controller routes
app.MapControllers();

// You can keep your minimal API endpoints alongside your controllers
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

// Example record (for the minimal API demo)
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
