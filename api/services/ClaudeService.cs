using Anthropic.SDK;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;

namespace SQLQueryAI.services;

public class ClaudeService : ISqlAiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<ClaudeService> _logger;
    private const string API_URL = "https://api.anthropic.com/v1/messages";
    private const string MODEL = "claude-3-sonnet-20240229";

    public ClaudeService(string apiKey, ILogger<ClaudeService> logger)
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
        _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        _apiKey = apiKey;
        _logger = logger;
    }

    public async Task<string> GenerateSqlQueryAsync(string userPrompt, string schemaContext)
    {
        try
        {
            // Create the message request using direct HTTP
            var request = new
            {
                model = MODEL,
                max_tokens = 1000,
                system = "You are a SQL assistant. Generate SQL queries based on the given schema and relationships." +
                         $"\n\nSchema and Relationships:\n{schemaContext}" +
                         "\n\nYou must respond with a valid JSON object that contains only a 'query' property with your SQL query as a string.",
                messages = new[]
                {
                    new { role = "user", content = userPrompt }
                }
            };

            // Send the request
            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(API_URL, content);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadFromJsonAsync<ClaudeResponse>();
            var responseText = jsonResponse?.Content?.FirstOrDefault()?.Text;

            if (string.IsNullOrEmpty(responseText))
            {
                throw new Exception("Claude returned an empty response");
            }

            // Parse the response to extract the SQL query
            try
            {
                using JsonDocument structuredJson = JsonDocument.Parse(responseText);
                _logger.LogInformation("Claude response {Query}", structuredJson.RootElement.GetProperty("query"));
                return structuredJson.RootElement.GetProperty("query").GetString() ?? "";
            }
            catch (JsonException)
            {
                // If Claude didn't return properly formatted JSON, try to extract just the SQL
                // This is a fallback mechanism
                _logger.LogWarning("Claude did not return valid JSON, attempting to parse raw response");

                // Simple extraction of SQL between triple backticks if present
                int startIndex = responseText.IndexOf("```sql");
                if (startIndex >= 0)
                {
                    startIndex += 6; // Move past the ```sql
                    int endIndex = responseText.IndexOf("```", startIndex);
                    if (endIndex > startIndex)
                    {
                        return responseText.Substring(startIndex, endIndex - startIndex).Trim();
                    }
                }

                // If all else fails, return the raw content and let the caller handle it
                return responseText;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating SQL query with Claude");
            throw;
        }
    }

    // Classes for Claude API responses
    private class ClaudeResponse
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Model { get; set; }
        public string Role { get; set; }
        public List<ClaudeContentPart> Content { get; set; }
    }

    private class ClaudeContentPart
    {
        public string Type { get; set; }
        public string Text { get; set; }
    }
}
