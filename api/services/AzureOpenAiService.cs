using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.Text.Json;

namespace SQLQueryAI.services;

public class AzureOpenAiService : ISqlAiService
{
  private readonly AzureOpenAIClient _azureClient;
    private readonly string _deploymentName;
    private readonly ILogger<AzureOpenAiService> _logger;

    public AzureOpenAiService(string apiKey, string endpoint, string deploymentName, ILogger<AzureOpenAiService> logger)
    {
        _azureClient = new AzureOpenAIClient(new Uri(endpoint), new Azure.AzureKeyCredential(apiKey));
        _deploymentName = deploymentName;
        _logger = logger;
    }    public async Task<string> GenerateSqlQueryAsync(string userPrompt, string schemaContext)
    {
        try
        {
            ChatClient chatClient = _azureClient.GetChatClient(_deploymentName);
            
            // Build the list of ChatMessages - using same prompt as OpenAiService
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
            };            // Send the chat request
            ChatCompletion completion = await chatClient.CompleteChatAsync(messages, options);

            // Parse the JSON to extract the "query" property
            string responseJson = completion.Content[0].Text;
            using JsonDocument structuredJson = JsonDocument.Parse(responseJson);

            _logger.LogInformation("Azure OpenAI response {Query}", structuredJson.RootElement.GetProperty("query"));

            // Return the SQL query
            return structuredJson.RootElement.GetProperty("query").GetString() ?? "";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating SQL query with Azure OpenAI");
            throw;
        }
    }
}