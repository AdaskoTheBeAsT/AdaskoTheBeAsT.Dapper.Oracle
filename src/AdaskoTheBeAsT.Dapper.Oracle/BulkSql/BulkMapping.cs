using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace AdaskoTheBeAsT.Dapper.Oracle.BulkSql;

/// <summary>
/// Contains mapping between a property on <typeparamref name="T"/> and a database query parameter.
/// </summary>
/// <typeparam name="T">Entity type for mapping.</typeparam>
[SuppressMessage(
    "Major Code Smell",
    "S107:Methods should not have too many parameters",
    Justification = "Preserves public Dapper.Oracle BulkMapping constructor signature compatibility.")]
public class BulkMapping<T>(
    string name,
    Func<T, object?> property,
    OracleMappingType? dbType = null,
    ParameterDirection? direction = null,
    int? size = null,
    bool? isNullable = null,
    byte? precision = null,
    byte? scale = null,
    string? sourceColumn = null,
    DataRowVersion? sourceVersion = null,
    OracleMappingCollectionType? collectionType = null,
    int[]? arrayBindSize = null)
{
    public string Name { get; set; } = name ?? throw new ArgumentNullException(nameof(name));

    public Func<T, object?>? Property { get; set; } = property;

    public ParameterDirection ParameterDirection { get; set; } = direction ?? ParameterDirection.Input;

    public OracleMappingType? DbType { get; set; } = dbType;

    public int? Size { get; set; } = size;

    public bool? IsNullable { get; set; } = isNullable ?? false;

    public byte? Precision { get; set; } = precision;

    public byte? Scale { get; set; } = scale;

    public string? SourceColumn { get; set; } = sourceColumn;

    public DataRowVersion SourceVersion { get; set; } = sourceVersion ?? DataRowVersion.Current;

    public OracleMappingCollectionType CollectionType { get; set; } =
        collectionType ?? OracleMappingCollectionType.None;

    [SuppressMessage(
        "Performance",
        "CA1819:Properties should not return arrays",
        Justification = "Matches OracleParameter.ArrayBindSize.")]
    public int[]? ArrayBindSize { get; set; } = arrayBindSize;
}
