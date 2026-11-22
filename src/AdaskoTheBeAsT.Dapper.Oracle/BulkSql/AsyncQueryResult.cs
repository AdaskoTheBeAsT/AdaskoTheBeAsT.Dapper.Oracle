namespace AdaskoTheBeAsT.Dapper.Oracle.BulkSql;

public class AsyncQueryResult
{
    /// <summary>
    /// Gets or sets the return value from Execute statement (the number of rows affected).
    /// </summary>
    public int ExecuteResult { get; set; }

    /// <summary>
    /// Gets or sets the dynamic parameters used in query.
    /// </summary>
    public OracleDynamicParameters? Parameters { get; set; }
}
