using OpenAI.Chat;
using System.Text.Json;

namespace SQLQueryAI.services;

public class OpenAiService : ISqlAiService
{
    private readonly ChatClient _client;
    private readonly ILogger<OpenAiService> _logger;

    public OpenAiService(string apiKey, ILogger<OpenAiService> logger)
    {
        _client = new ChatClient(model: "gpt-4o", apiKey: apiKey);
        _logger = logger;
    }

    public async Task<string> GenerateSqlQueryAsync(string userPrompt, string schemaContext)
    {
        // Build the list of ChatMessages
        List<ChatMessage> messages = new()
        {
            new SystemChatMessage("You are a SQL assistant. Generate SQL queries based on the given schema and relationships."),
            new SystemChatMessage($"Schema and Relationships:\n{schemaContext}"),
            new UserChatMessage(userPrompt)
        };

        // Define a JSON schema to force structured output
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

        // Send the chat request
        ChatCompletion completion = await _client.CompleteChatAsync(messages, options);

        // Parse the JSON to extract the "query" property
        string responseJson = completion.Content[0].Text;
        using JsonDocument structuredJson = JsonDocument.Parse(responseJson);

        _logger.LogInformation("OpenAI response {Query}", structuredJson.RootElement.GetProperty("query"));

        // Return the SQL query
        return structuredJson.RootElement.GetProperty("query").GetString() ?? "";
    }
}
