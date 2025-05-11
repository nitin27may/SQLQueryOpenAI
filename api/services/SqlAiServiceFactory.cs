namespace SQLQueryAI.services;

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
            ProviderType.OpenAI => new OpenAiService(config.ApiKey, logger as ILogger<OpenAiService>),
            ProviderType.AzureOpenAI => new AzureOpenAiService(
                config.ApiKey,
                config.Endpoint,
                config.DeploymentName,
                logger as ILogger<AzureOpenAiService>),
            ProviderType.Claude => new ClaudeService(config.ApiKey, logger as ILogger<ClaudeService>),
            ProviderType.Gemini => new GeminiService(config.ApiKey, logger as ILogger<GeminiService>),
            _ => throw new ArgumentException($"Unsupported provider type: {providerType}")
        };
    }
}
