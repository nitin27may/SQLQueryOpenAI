using Dapper;
using Microsoft.Data.SqlClient;
using System.Text;

namespace AIQueryBuilder;

public class DatabaseService
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseService> _logger;

    public DatabaseService(string connectionString, ILogger<DatabaseService> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    public string GetSchemaAndRelationships()
    {
        using var connection = new SqlConnection(_connectionString);

        // Fetch schema details
        var schemaQuery = @"
        SELECT
            TABLE_SCHEMA AS SchemaName,
            TABLE_NAME AS TableName,
            COLUMN_NAME AS ColumnName,
            DATA_TYPE AS DataType,
            CHARACTER_MAXIMUM_LENGTH AS MaxLength,
            IS_NULLABLE AS IsNullable,
            COLUMN_DEFAULT AS DefaultValue
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA != 'sys'
        ORDER BY TABLE_SCHEMA, TABLE_NAME, ORDINAL_POSITION;";

        var schemaDetails = connection.Query<dynamic>(schemaQuery);

        // Fetch foreign key relationships
        var relationshipsQuery = @"
        SELECT
            fk.name AS ForeignKeyName,
            tp.name AS ParentTable,
            cp.name AS ParentColumn,
            tr.name AS ReferencedTable,
            cr.name AS ReferencedColumn
        FROM sys.foreign_keys AS fk
        INNER JOIN sys.foreign_key_columns AS fkc ON fk.object_id = fkc.constraint_object_id
        INNER JOIN sys.tables AS tp ON fkc.parent_object_id = tp.object_id
        INNER JOIN sys.columns AS cp ON fkc.parent_object_id = cp.object_id AND fkc.parent_column_id = cp.column_id
        INNER JOIN sys.tables AS tr ON fkc.referenced_object_id = tr.object_id
        INNER JOIN sys.columns AS cr ON fkc.referenced_object_id = cr.object_id AND fkc.referenced_column_id = cr.column_id
        ORDER BY ParentTable, ReferencedTable;";

        try
        {
            connection.Open();
            var relationships = connection.Query<dynamic>(relationshipsQuery);

            // Format the output
            var sb = new StringBuilder();

            sb.AppendLine("Schema Details:");
            foreach (var row in schemaDetails)
            {
                sb.AppendLine($"Table: {row.TableName}, Column: {row.ColumnName}, DataType: {row.DataType}, MaxLength: {row.MaxLength}, IsNullable: {row.IsNullable}, DefaultValue: {row.DefaultValue}");
            }

            sb.AppendLine("\nRelationships:");
            foreach (var rel in relationships)
            {
                sb.AppendLine($"ForeignKey: {rel.ForeignKeyName}, ParentTable: {rel.ParentTable}, ParentColumn: {rel.ParentColumn}, ReferencedTable: {rel.ReferencedTable}, ReferencedColumn: {rel.ReferencedColumn}");
            }

            return sb.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening connection to database.");
            return "Error fetching schema and relationships.";
        }
    }

    /// <summary>
    /// Executes a SQL query using Dapper and returns the result set as an IEnumerable of dynamic objects.
    /// Exceptions are caught and rethrown with context to centralize error handling.
    /// </summary>
    /// <param name="sqlQuery">The SQL query to execute.</param>
    /// <returns>Result set as IEnumerable of dynamic rows.</returns>
    public async Task<IEnumerable<dynamic>> ExecuteQueryAsync(string sqlQuery)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // If you want to log or validate the query, do so here.
            var resultSet = await connection.QueryAsync(sqlQuery);
            return resultSet;
        }
        catch (Exception ex)
        {
            // Wrap and rethrow to provide additional context
            throw new Exception($"Error executing query: {ex.Message}", ex);
        }
    }
}