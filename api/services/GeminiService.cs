using System.Text;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Json;

namespace SQLQueryAI.services;

public class GeminiService : ISqlAiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<GeminiService> _logger;
    private const string API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent";

    public GeminiService(string apiKey, ILogger<GeminiService> logger)
    {
        _httpClient = new HttpClient();
        _apiKey = apiKey;
        _logger = logger;
    }

    public async Task<string> GenerateSqlQueryAsync(string userPrompt, string schemaContext)
    {
        try
        {
            // Create the request URL with API key
            string requestUrl = $"{API_URL}?key={_apiKey}";

            // Create the request body
            var request = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new
                            {
                                text = $"You are a SQL assistant. Generate a SQL query based on the given schema and relationships. " +
                                      $"Schema and Relationships:\n{schemaContext}\n\n" +
                                      $"User request: {userPrompt}\n\n" +
                                      $"Return ONLY a JSON object with a single 'query' field containing your SQL query as a string, nothing else."
                            }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.0,
                    topP = 0.95,
                    maxOutputTokens = 1000
                }
            };

            // Send the request
            var response = await _httpClient.PostAsJsonAsync(requestUrl, request);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadFromJsonAsync<GeminiResponse>();
            var responseText = jsonResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

            if (string.IsNullOrEmpty(responseText))
            {
                throw new Exception("Gemini returned an empty response");
            }

            // Parse the response to extract the SQL query
            try
            {
                using JsonDocument structuredJson = JsonDocument.Parse(responseText);
                _logger.LogInformation("Gemini response {Query}", structuredJson.RootElement.GetProperty("query"));
                return structuredJson.RootElement.GetProperty("query").GetString() ?? "";
            }
            catch (JsonException)
            {
                // If Gemini didn't return properly formatted JSON, try to extract just the SQL
                _logger.LogWarning("Gemini did not return valid JSON, attempting to parse raw response");

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
            _logger.LogError(ex, "Error generating SQL query with Gemini");
            throw;
        }
    }

    // Classes for Gemini API responses
    private class GeminiResponse
    {
        public List<GeminiCandidate> Candidates { get; set; }
    }

    private class GeminiCandidate
    {
        public GeminiContent Content { get; set; }
    }

    private class GeminiContent
    {
        public List<GeminiPart> Parts { get; set; }
    }

    private class GeminiPart
    {
        public string Text { get; set; }
    }
}