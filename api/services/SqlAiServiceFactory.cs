namespace SQLQueryAI.services;
using Microsoft.Extensions.Logging;

public class SqlAiServiceFactory
{
    public enum ProviderType
    {
        OpenAI,
        AzureOpenAI,
        Claude,
        Gemini
    }

    public static ISqlAiService CreateService(
        ProviderType providerType,
        AiServiceConfiguration config,
        ILogger logger)
    {
        return providerType switch
        {            
            ProviderType.OpenAI => new OpenAiService(config.ApiKey ?? "", logger as ILogger<OpenAiService> ?? CreateLogger<OpenAiService>()),
            ProviderType.AzureOpenAI => new AzureOpenAiService(
                config.ApiKey ?? "",
                config.Endpoint ?? "",
                config.DeploymentName ?? "",
                logger as ILogger<AzureOpenAiService> ?? CreateLogger<AzureOpenAiService>()),
            ProviderType.Claude => new ClaudeService(config.ApiKey ?? "", logger as ILogger<ClaudeService> ?? CreateLogger<ClaudeService>()),
            ProviderType.Gemini => new GeminiService(config.ApiKey ?? "", logger as ILogger<GeminiService> ?? CreateLogger<GeminiService>()),
            _ => throw new ArgumentException($"Unsupported provider type: {providerType}")
        };
    }

    private static ILogger<T> CreateLogger<T>()
    {
        // Create a default logger factory and logger if none provided
        var loggerFactory = LoggerFactory.Create(builder => 
            builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        return loggerFactory.CreateLogger<T>();
    }
}
