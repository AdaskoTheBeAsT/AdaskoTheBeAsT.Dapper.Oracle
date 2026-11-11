namespace AdaskoTheBeAsT.Dapper.Oracle;

/// <summary>
/// BulkMapping enum to map Parameter status for OracleParameter.
/// </summary>
public enum OracleParameterMappingStatus
{
    Success,
    NullFetched,
    NullInsert,
    Truncation,
}
