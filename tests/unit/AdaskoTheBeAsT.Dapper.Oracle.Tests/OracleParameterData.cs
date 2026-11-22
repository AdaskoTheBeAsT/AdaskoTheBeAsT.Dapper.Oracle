using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace AdaskoTheBeAsT.Dapper.Oracle.Tests;

public class OracleParameterData
{
    public string? ParameterName { get; set; }

    public string? OracleDbType { get; set; }

    public string? CollectionType { get; set; }

    public object? Value { get; set; }

    public ParameterDirection Direction { get; set; }

    public int Size { get; set; }

    public bool IsNullable { get; set; }

    public int Scale { get; set; }

    public int Precision { get; set; }

    public string? SourceColumn { get; set; }

    [SuppressMessage(
        "Performance",
        "CA1819:Properties should not return arrays",
        Justification = "Mirrors OracleParameter.ArrayBindSize for assertions.")]
    public int[]? ArrayBindSize { get; set; }

    public DataRowVersion SourceVersion { get; set; }
}
