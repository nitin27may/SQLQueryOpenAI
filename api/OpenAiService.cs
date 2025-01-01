using OpenAI.Chat;
using System.Text.Json;

namespace SQLQueryAI;

/// <summary>
/// A service that uses the OpenAI Chat API to generate SQL queries
/// with structured output (JSON schema) via GPT-4o.
/// </summary>
public class OpenAiService
{
    private readonly ChatClient _client;
    private readonly ILogger<OpenAiService> _logger;

    /// <summary>
    /// Creates an instance of OpenAiService with a ChatClient configured for GPT-4o.
    /// </summary>
    /// <param name="apiKey">Your OpenAI API key</param>
    public OpenAiService(string apiKey, ILogger<OpenAiService> logger)
    {
        _client = new ChatClient(model: "gpt-4o", apiKey: apiKey);
        _logger = logger;
    }

    /// <summary>
    /// Generates a SQL query in a structured JSON format based on the provided user prompt and schema context.
    /// </summary>
    /// <param name="userPrompt">The user's natural language request or question.</param>
    /// <param name="schemaContext">The database schema or relationship info to provide contextual knowledge.</param>
    /// <returns>A fully-formed SQL query from the model response.</returns>
    public async Task<string> GenerateSqlQueryAsync(string userPrompt, string schemaContext)
    {
        // 1. Build the list of ChatMessages
        List<ChatMessage> messages = new()
            {
                new SystemChatMessage("You are a SQL assistant. Generate SQL queries based on the given schema and relationships."),
                new SystemChatMessage($"Schema and Relationships:\n{schemaContext}"),
                new UserChatMessage(userPrompt)
            };

        // 2. Define a JSON schema to force structured output
        ChatCompletionOptions options = new()
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                jsonSchemaFormatName: "sql_query_generation",
                jsonSchema: BinaryData.FromBytes("""
                    {
                      "type": "object",
                      "properties": {
                        "query": {
                          "type": "string",
                          "description": "A fully-formed SQL query that satisfies the user request."
                        }
                      },
                      "required": ["query"],
                      "additionalProperties": false
                    }
                    """u8.ToArray()),
                jsonSchemaIsStrict: true
            )
        };

        // 3. Send the chat request
        ChatCompletion completion = _client.CompleteChat(messages, options);

        // 4. Parse the JSON to extract the "query" property
        string responseJson = completion.Content[0].Text;
        using JsonDocument structuredJson = JsonDocument.Parse(responseJson);

        _logger.LogInformation("responseJson", structuredJson.RootElement.GetProperty("query"));
        // 5. Return the SQL query
        return structuredJson.RootElement.GetProperty("query").GetString();
    }
}