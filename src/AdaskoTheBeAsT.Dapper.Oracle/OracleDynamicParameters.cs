using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AdaskoTheBeAsT.Dapper.Oracle.Util;
using Dapper;

namespace AdaskoTheBeAsT.Dapper.Oracle;

/// <summary>
/// Parameter support for Oracle-specific types and functions. For use with Dapper.
/// Implements <see cref="SqlMapper.IDynamicParameters"/>.
/// </summary>
[SuppressMessage(
    "Naming",
    "CA1710:Identifiers should have correct suffix",
    Justification = "Matches Dapper DynamicParameters naming convention.")]
public class OracleDynamicParameters
    : SqlMapper.IDynamicParameters,
        IEnumerable<KeyValuePair<string, OracleDynamicParameters.OracleParameterInfo>>
{
    private static readonly ConcurrentDictionary<SqlMapper.Identity, Action<IDbCommand, object>> ParamReaderCache =
        new();

    private List<object>? _templates;

    public OracleDynamicParameters()
    {
    }

    public OracleDynamicParameters(object? template)
    {
        AddDynamicParams(template);
    }

    public int ArrayBindCount { get; set; }

    /// <summary>
    /// Gets or sets the initial LOB fetch size on <c>OracleCommand</c>.
    /// Set to <c>-1</c> (Oracle ODP.NET recommendation) to fetch entire <c>CLOB</c>/<c>NCLOB</c>/<c>BLOB</c> values inline without extra round-trips.
    /// </summary>
    public int? InitialLOBFetchSize { get; set; }

    /// <summary>
    /// Gets or sets the initial <c>LONG</c> / <c>LONG RAW</c> fetch size on <c>OracleCommand</c> (<c>-1</c> fetches the entire value).
    /// </summary>
    public int? InitialLONGFetchSize { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether ODP.NET binds parameters by name instead of position.
    /// </summary>
    public bool? BindByName { get; set; }

    /// <summary>
    /// Gets or sets the result-set fetch buffer size in bytes on <c>OracleCommand</c>.
    /// </summary>
    public long? FetchSize { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the command is cached in the ODP.NET statement cache.
    /// Set to <see langword="false"/> for non-reusable dynamic SQL to avoid statement-cache eviction.
    /// </summary>
    public bool? AddToStatementCache { get; set; }

    public IEnumerable<string> ParameterNames => Parameters.Select(static p => p.Key);

    private Dictionary<string, OracleParameterInfo> Parameters { get; } = new(StringComparer.Ordinal);

    public void AddDynamicParams(dynamic? param)
    {
        if (param is not object obj)
        {
            return;
        }

        if (obj is OracleDynamicParameters subDynamic)
        {
            foreach (var kvp in subDynamic.Parameters)
            {
                Parameters.Add(kvp.Key, kvp.Value);
            }

            if (subDynamic._templates != null)
            {
                _templates ??= [];
                _templates.AddRange(subDynamic._templates);
            }

            return;
        }

        if (obj is IEnumerable<KeyValuePair<string, object?>> dictionary)
        {
            foreach (var kvp in dictionary)
            {
                Add(kvp.Key, kvp.Value);
            }

            return;
        }

        _templates ??= [];
        _templates.Add(obj);
    }

    public void Add(
        string name,
        object? value = null,
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
        ThrowHelper.ThrowIfNull(name, nameof(name));

        var cleanedName = Clean(name);
        Parameters[cleanedName] = new OracleParameterInfo
        {
            Name = cleanedName,
            Value = value,
            ParameterDirection = direction ?? ParameterDirection.Input,
            DbType = dbType,
            Size = size,
            IsNullable = isNullable ?? false,
            Precision = precision,
            Scale = scale,
            SourceColumn = sourceColumn,
            SourceVersion = sourceVersion ?? DataRowVersion.Current,
            CollectionType = collectionType ?? OracleMappingCollectionType.None,
            ArrayBindSize = arrayBindSize,
        };
    }

    public T Get<T>(string name)
    {
        var paramInfo = GetParameter(name);
        var attachedParam = paramInfo.AttachedParam;
        var val = attachedParam == null ? paramInfo.Value : attachedParam.Value;
        if (val == DBNull.Value)
        {
            if (typeof(T).IsValueType
                && Nullable.GetUnderlyingType(typeof(T)) == null)
            {
                throw new InvalidOperationException(
                    "Attempting to cast a DBNull to a non nullable type! Note that out/return parameters will not have updated values until the data stream completes (after the 'foreach' for Query(..., buffered: false), or after the GridReader has been disposed for QueryMultiple)");
            }

            return default!;
        }

        return OracleValueConverter.Convert<T>(val);
    }

    public OracleParameterInfo GetParameter(string name)
    {
        ThrowHelper.ThrowIfNull(name, nameof(name));

        return Parameters[Clean(name)];
    }

    public IEnumerator<KeyValuePair<string, OracleParameterInfo>> GetEnumerator() => Parameters.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Parameters.GetEnumerator();

    void SqlMapper.IDynamicParameters.AddParameters(
        IDbCommand command,
        SqlMapper.Identity identity)
    {
        AddParameters(command, identity);
    }

    protected virtual void AddParameters(
        IDbCommand command,
        SqlMapper.Identity? identity)
    {
        ThrowHelper.ThrowIfNull(command, nameof(command));

        ApplyCommandProperties(command);
        AddTemplateParameters(command, identity);

        foreach (var param in Parameters.Values)
        {
            AddSingleParameter(command, param);
        }
    }

    private static string Clean(string name)
    {
        if (!string.IsNullOrEmpty(name)
            && (name[0] == '@' || name[0] == ':' || name[0] == '?'))
        {
            return name.Substring(1);
        }

        return name;
    }

    private static void AddSingleParameter(
        IDbCommand command,
        OracleParameterInfo param)
    {
        var name = Clean(param.Name);
        var add = !command.Parameters.Contains(name);
        IDbDataParameter p;
        if (add)
        {
            p = command.CreateParameter();
            p.ParameterName = name;
        }
        else
        {
            p = command.Parameters[name] as IDbDataParameter
                ?? throw new InvalidOperationException($"Parameter '{name}' is not an IDbDataParameter.");
        }

        OracleMethodHelper.SetOracleParameters(p, param);
        p.Direction = param.ParameterDirection;

        var val = param.Value;
        if (val != null
            && OracleTypeMapper.HasTypeHandler(val.GetType(), out var handler)
            && handler != null)
        {
            handler.SetValue(p, val);
        }
        else
        {
            p.Value = val ?? DBNull.Value;

            if (param.Size != null)
            {
                p.Size = param.Size.Value;
            }
            else
            {
                ApplyDefaultParameterSize(p, param, val);
            }
        }

        if (add)
        {
            command.Parameters.Add(p);
            param.AttachedParam = p;
        }
    }

    private static void ApplyDefaultParameterSize(
        IDbDataParameter parameter,
        OracleParameterInfo param,
        object? value)
    {
        if (value is string s)
        {
            if (param.DbType is OracleMappingType.Clob
                or OracleMappingType.NClob
                or OracleMappingType.Json
                or OracleMappingType.XmlType
                or OracleMappingType.Long)
            {
                return;
            }

            if (param.DbType is OracleMappingType.NVarchar2 or OracleMappingType.NChar)
            {
                if (s.Length <= 2000)
                {
                    parameter.Size = 2000;
                }
                else if (s.Length <= 16383)
                {
                    parameter.Size = 16383;
                }

                return;
            }

            if (s.Length <= 4000)
            {
                parameter.Size = 4000;
            }
            else if (s.Length <= 32767)
            {
                parameter.Size = 32767;
            }

            return;
        }

        if (value == null
            && param.ParameterDirection is ParameterDirection.Output
                or ParameterDirection.InputOutput
                or ParameterDirection.ReturnValue)
        {
            parameter.Size = param.DbType switch
            {
                OracleMappingType.Varchar2 or OracleMappingType.Char => 4000,
                OracleMappingType.NVarchar2 or OracleMappingType.NChar => 2000,
                OracleMappingType.Raw => 2000,
                _ => parameter.Size,
            };
        }
    }

    private void ApplyCommandProperties(IDbCommand command)
    {
        if (ArrayBindCount > 0)
        {
            OracleMethodHelper.SetArrayBindCount(command, ArrayBindCount);
        }

        if (InitialLOBFetchSize.HasValue)
        {
            OracleMethodHelper.SetInitialLOBFetchSize(command, InitialLOBFetchSize.Value);
        }

        if (InitialLONGFetchSize.HasValue)
        {
            OracleMethodHelper.SetInitialLONGFetchSize(command, InitialLONGFetchSize.Value);
        }

        if (BindByName.HasValue)
        {
            OracleMethodHelper.SetBindByName(command, BindByName.Value);
        }

        if (FetchSize.HasValue)
        {
            OracleMethodHelper.SetFetchSize(command, FetchSize.Value);
        }

        if (AddToStatementCache.HasValue)
        {
            OracleMethodHelper.SetAddToStatementCache(command, AddToStatementCache.Value);
        }
    }

    private void AddTemplateParameters(
        IDbCommand command,
        SqlMapper.Identity? identity)
    {
        if (_templates == null)
        {
            return;
        }

        var resolvedIdentity = ThrowHelper.EnsureNotNull(identity, nameof(identity));

        foreach (var template in _templates)
        {
            var newIdent = resolvedIdentity.ForDynamicParameters(template.GetType());
            var appender = ParamReaderCache.GetOrAdd(
                newIdent,
                static id => SqlMapper.CreateParamInfoGenerator(id, checkForDuplicates: false, removeUnused: false));
            appender(command, template);
        }
    }

    /// <summary>
    /// Class used to pass parameter information to and from the OracleCommand object.
    /// </summary>
    public class OracleParameterInfo
    {
        public string Name { get; set; } = string.Empty;

        public object? Value { get; set; }

        public ParameterDirection ParameterDirection { get; set; }

        public OracleMappingType? DbType { get; set; }

        public int? Size { get; set; }

        public bool? IsNullable { get; set; }

        public byte? Precision { get; set; }

        public byte? Scale { get; set; }

        public string? SourceColumn { get; set; } = string.Empty;

        public DataRowVersion SourceVersion { get; set; }

        public OracleMappingCollectionType CollectionType { get; set; } = OracleMappingCollectionType.None;

        [SuppressMessage(
            "Performance",
            "CA1819:Properties should not return arrays",
            Justification = "Matches OracleParameter.ArrayBindSize.")]
        public int[]? ArrayBindSize { get; set; }

        public OracleParameterMappingStatus Status { get; set; }

        public IDbDataParameter? AttachedParam { get; set; }
    }
}
