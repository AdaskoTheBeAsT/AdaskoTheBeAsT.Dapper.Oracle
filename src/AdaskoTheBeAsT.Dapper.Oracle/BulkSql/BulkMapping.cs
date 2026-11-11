using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace AdaskoTheBeAsT.Dapper.Oracle.BulkSql;

/// <summary>
/// Contains mapping between a property on <typeparamref name="T"/> and a database query parameter.
/// </summary>
/// <typeparam name="T">Entity type for mapping.</typeparam>
public class BulkMapping<T>
{
    public BulkMapping(
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
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Property = property;
        ParameterDirection = direction ?? ParameterDirection.Input;
        DbType = dbType;
        Size = size;
        IsNullable = isNullable ?? false;
        Precision = precision;
        Scale = scale;
        SourceColumn = sourceColumn;
        SourceVersion = sourceVersion ?? DataRowVersion.Current;
        CollectionType = collectionType ?? OracleMappingCollectionType.None;
        ArrayBindSize = arrayBindSize;
    }

    public string Name { get; set; }

    public Func<T, object?>? Property { get; set; }

    public ParameterDirection ParameterDirection { get; set; }

    public OracleMappingType? DbType { get; set; }

    public int? Size { get; set; }

    public bool? IsNullable { get; set; }

    public byte? Precision { get; set; }

    public byte? Scale { get; set; }

    public string? SourceColumn { get; set; }

    public DataRowVersion SourceVersion { get; set; }

    public OracleMappingCollectionType CollectionType { get; set; }

    [SuppressMessage(
        "Performance",
        "CA1819:Properties should not return arrays",
        Justification = "Matches OracleParameter.ArrayBindSize.")]
    public int[]? ArrayBindSize { get; set; }
}
