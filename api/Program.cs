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

// Check environment variables for provider type first, then fallback to appsettings
var providerTypeStr = Environment.GetEnvironmentVariable("AI_PROVIDER_TYPE") ?? aiConfig["ProviderType"];

if (!Enum.TryParse<ProviderType>(providerTypeStr, out var providerType))
{
    providerType = ProviderType.OpenAI; // Default to OpenAI if not specified or invalid
}

// Register the AI service factory and configuration
builder.Services.AddSingleton<ISqlAiService>(sp =>
{    // Create the appropriate configuration based on provider type
    var aiServiceConfig = new AiServiceConfiguration
    {
        ApiKey = providerType switch
        {
            ProviderType.OpenAI => Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? aiConfig["OpenAI:ApiKey"] ?? "",
            ProviderType.AzureOpenAI => Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? aiConfig["AzureOpenAI:ApiKey"] ?? "",
            ProviderType.Claude => Environment.GetEnvironmentVariable("CLAUDE_API_KEY") ?? aiConfig["Claude:ApiKey"] ?? "",
            ProviderType.Gemini => Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? aiConfig["Gemini:ApiKey"] ?? "",
            _ => Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? aiConfig["OpenAI:ApiKey"] ?? ""
        },
        Endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? aiConfig["AzureOpenAI:Endpoint"] ?? "",
        DeploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? aiConfig["AzureOpenAI:DeploymentName"] ?? "",
        Model = providerType switch
        {
            ProviderType.OpenAI => Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? aiConfig["OpenAI:Model"] ?? "gpt-4o",
            ProviderType.AzureOpenAI => Environment.GetEnvironmentVariable("AZURE_OPENAI_MODEL") ?? aiConfig["AzureOpenAI:Model"] ?? "gpt-4",
            ProviderType.Claude => Environment.GetEnvironmentVariable("CLAUDE_MODEL") ?? aiConfig["Claude:Model"] ?? "claude-3-sonnet-20240229",
            ProviderType.Gemini => Environment.GetEnvironmentVariable("GEMINI_MODEL") ?? aiConfig["Gemini:Model"] ?? "gemini-pro",
            _ => Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? aiConfig["OpenAI:Model"] ?? "gpt-4o"
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
string connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION") ?? 
                         builder.Configuration["ConnectionStrings:DefaultConnection"] ?? "";
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
