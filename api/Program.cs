using AIQueryBuilder;

var builder = WebApplication.CreateBuilder(args);

// 1. Add services to the container.
builder.Services.AddOpenApi(); // For minimal APIs or MVC, depending on your approach

// ADD THIS to enable controllers
builder.Services.AddControllers();

// OpenAI API key from appsettings.json or secrets
string openAiApiKey = builder.Configuration["OpenAI:ApiKey"];
builder.Services.AddSingleton<OpenAiService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<OpenAiService>>();
    return new OpenAiService(openAiApiKey, logger);
});

// Database connection string
string connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"];
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
