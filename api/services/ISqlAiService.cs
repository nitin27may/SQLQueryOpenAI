namespace SQLQueryAI.services;

public interface ISqlAiService
{
    Task<string> GenerateSqlQueryAsync(string userPrompt, string schemaContext);
}