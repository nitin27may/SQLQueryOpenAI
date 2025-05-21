using Microsoft.AspNetCore.Mvc;
using SQLQueryAI.services;

namespace SQLQueryAI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SqlController : ControllerBase
{
    private readonly ISqlAiService _sqlAiService;
    private readonly DatabaseService _databaseService;
    private readonly ILogger<SqlController> _logger;
    // Example: a service that fetches your DB schema, relationships, etc.

    public SqlController(ISqlAiService sqlAiService, DatabaseService databaseService, ILogger<SqlController> logger)
    {
        _sqlAiService = sqlAiService;
        _databaseService = databaseService;
        _logger = logger;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateSqlQuery([FromBody] string userPrompt)
    {
        try
        {
            // Suppose this returns a string describing your DB schema & relationships
            string schemaContext = _databaseService.GetSchemaAndRelationships();

            // Generate the structured SQL query
            string sqlQuery = await _sqlAiService.GenerateSqlQueryAsync(userPrompt, schemaContext);

            // Return the query
            return Ok(new { Query = sqlQuery });
        }
        catch (System.Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("execute")]
    public async Task<IActionResult> Execute([FromBody] string sqlQuery)
    {
        try
        {
            // Generate the structured SQL query
            var results = await _databaseService.ExecuteQueryAsync(sqlQuery);

            // Return the query
            return Ok(new { Results = results });
        }
        catch (System.Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Combined method: Generates the SQL from the user prompt AND executes it in one go.
    /// Returns both the generated SQL and the execution result set.
    /// </summary>
    /// <param name="userPrompt">User's natural language request.</param>
    /// <returns>The generated SQL and the execution results.</returns>
    [HttpPost("generate-and-execute")]
    public async Task<IActionResult> GenerateAndExecuteSql([FromBody] string userPrompt)
    {
        try
        {
            _logger.LogInformation("Generating SQL query for user prompt: {UserPrompt}", userPrompt);
            // 1. Get schema context
            string schemaContext = _databaseService.GetSchemaAndRelationships();

            // 2. Generate the SQL
            string sqlQuery = await _sqlAiService.GenerateSqlQueryAsync(userPrompt, schemaContext);

            // 3. Execute the generated query
            IEnumerable<dynamic> results = await _databaseService.ExecuteQueryAsync(sqlQuery);

            // 4. Return both
            return Ok(new
            {
                Query = sqlQuery,
                Results = results
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
}