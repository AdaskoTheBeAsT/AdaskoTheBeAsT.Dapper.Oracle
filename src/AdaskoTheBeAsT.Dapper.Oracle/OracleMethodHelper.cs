using System;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using AdaskoTheBeAsT.Dapper.Oracle.Expressions;
using AdaskoTheBeAsT.Dapper.Oracle.Util;

namespace AdaskoTheBeAsT.Dapper.Oracle;

internal static class OracleMethodHelper
{
    private static readonly ConcurrentDictionary<Type, OracleParameterExpressions> CachedOracleTypes = new();
    private static readonly ConcurrentDictionary<Type, CommandExpressions> CachedOracleCommandProperties = new();

    public static void SetArrayBindCount(
        IDbCommand command,
        int arrayBindCount)
    {
        ThrowHelper.ThrowIfNull(command, nameof(command));

        var actual = command.IsWrapped() ? command.DownCastCommand() : command;
        Get(actual).ArrayBindCount.SetValue(actual, arrayBindCount);
    }

    public static void SetInitialLOBFetchSize(
        IDbCommand command,
        int initialLobFetchSize)
    {
        ThrowHelper.ThrowIfNull(command, nameof(command));

        var actual = command.IsWrapped() ? command.DownCastCommand() : command;
        Get(actual).InitialLOBFetchSize.SetValue(actual, initialLobFetchSize);
    }

    public static void SetInitialLONGFetchSize(
        IDbCommand command,
        int initialLongFetchSize)
    {
        ThrowHelper.ThrowIfNull(command, nameof(command));

        var actual = command.IsWrapped() ? command.DownCastCommand() : command;
        Get(actual).InitialLONGFetchSize.SetValue(actual, initialLongFetchSize);
    }

    public static void SetBindByName(
        IDbCommand command,
        bool bindByName)
    {
        ThrowHelper.ThrowIfNull(command, nameof(command));

        var actual = command.IsWrapped() ? command.DownCastCommand() : command;
        Get(actual).BindByName.SetValue(actual, bindByName);
    }

    public static void SetFetchSize(
        IDbCommand command,
        long fetchSize)
    {
        ThrowHelper.ThrowIfNull(command, nameof(command));

        var actual = command.IsWrapped() ? command.DownCastCommand() : command;
        Get(actual).FetchSize.SetValue(actual, fetchSize);
    }

    public static void SetAddToStatementCache(
        IDbCommand command,
        bool addToStatementCache)
    {
        ThrowHelper.ThrowIfNull(command, nameof(command));

        var actual = command.IsWrapped() ? command.DownCastCommand() : command;
        Get(actual).AddToStatementCache.SetValue(actual, addToStatementCache);
    }

    public static void SetOracleParameters(
        IDbDataParameter parameter,
        OracleDynamicParameters.OracleParameterInfo oracleParameterInfo)
    {
        ThrowHelper.ThrowIfNull(parameter, nameof(parameter));
        ThrowHelper.ThrowIfNull(oracleParameterInfo, nameof(oracleParameterInfo));

        var method = CachedOracleTypes.GetOrAdd(parameter.GetType(), GetOracleProperties);

        if (oracleParameterInfo.DbType.HasValue)
        {
            method.OraDbType.SetValue(parameter, oracleParameterInfo.DbType.Value);
        }

        if (oracleParameterInfo.IsNullable.HasValue)
        {
            method.IsNullable.SetValue(parameter, oracleParameterInfo.IsNullable.Value);
        }

        if (oracleParameterInfo.Scale.HasValue)
        {
            parameter.Scale = oracleParameterInfo.Scale.Value;
        }

        if (oracleParameterInfo.Precision.HasValue)
        {
            parameter.Precision = oracleParameterInfo.Precision.Value;
        }

        parameter.SourceVersion = oracleParameterInfo.SourceVersion;

        if (oracleParameterInfo.SourceColumn != null)
        {
            parameter.SourceColumn = oracleParameterInfo.SourceColumn;
        }

        if (oracleParameterInfo.CollectionType != OracleMappingCollectionType.None)
        {
            method.CollectionType.SetValue(parameter, oracleParameterInfo.CollectionType);
        }

        if (oracleParameterInfo.ArrayBindSize != null)
        {
            method.ArrayBindSize.SetValue(parameter, oracleParameterInfo.ArrayBindSize);
        }
    }

    internal static OracleDynamicParameters.OracleParameterInfo GetParameterInfo(IDbDataParameter parameter)
    {
        ThrowHelper.ThrowIfNull(parameter, nameof(parameter));

        var method = GetOracleProperties(parameter.GetType());
        return new OracleDynamicParameters.OracleParameterInfo
        {
            Name = parameter.ParameterName,
            SourceVersion = parameter.SourceVersion,
            Precision = parameter.Precision,
            Size = parameter.Size,
            DbType = method.OraDbType.GetValue(parameter),
            ArrayBindSize = method.ArrayBindSize.GetValue(parameter),
            CollectionType = method.CollectionType.GetValue(parameter),
            ParameterDirection = parameter.Direction,
            IsNullable = parameter.IsNullable,
            Scale = parameter.Scale,
            SourceColumn = parameter.SourceColumn,
            Status = method.Status.GetValue(parameter),
            Value = parameter.Value,
        };
    }

    private static OracleParameterExpressions GetOracleProperties(Type type)
    {
        return new OracleParameterExpressions(type);
    }

    private static CommandExpressions Get(IDbCommand command)
    {
        return CachedOracleCommandProperties.GetOrAdd(command.GetType(), static type => new CommandExpressions(type));
    }

    private sealed class OracleParameterExpressions
    {
        [SuppressMessage(
            "Design",
            "CC0021:Use nameof",
            Justification = "Refers to external ODP.NET namespace prefix, not AdaskoTheBeAsT.Dapper.Oracle.")]
        [SuppressMessage(
            "Style",
            "CC0021:Use nameof",
            Justification = "Refers to external ODP.NET namespace prefix, not AdaskoTheBeAsT.Dapper.Oracle.")]
        public OracleParameterExpressions(Type oracleParameterType)
        {
            var ns = oracleParameterType.Namespace ?? string.Empty;
            if (!ns.StartsWith("Oracle", StringComparison.Ordinal)
                && !ns.StartsWith("Castle", StringComparison.Ordinal))
            {
                throw new NotSupportedException(
                    $"Whoopsies! This library will only work with Oracle types, you are attempting to use type {oracleParameterType.FullName}.");
            }

            OraDbType = new ObjectEnumWrapper<IDbDataParameter, OracleMappingType>(
                "OracleDbType",
                "OracleDbType",
                oracleParameterType);
            ArrayBindSize = new ObjectWrapper<IDbDataParameter, int[]?>(nameof(ArrayBindSize), oracleParameterType);
            CollectionType = new ObjectEnumWrapper<IDbDataParameter, OracleMappingCollectionType>(
                "OracleCollectionType",
                nameof(CollectionType),
                oracleParameterType);
            Status = new ObjectEnumWrapper<IDbDataParameter, OracleParameterMappingStatus>(
                nameof(Status),
                nameof(Status),
                oracleParameterType);
            IsNullable = new ObjectWrapper<IDbDataParameter, bool>(nameof(IsNullable), oracleParameterType);
        }

        public ObjectEnumWrapper<IDbDataParameter, OracleMappingType> OraDbType { get; }

        public ObjectWrapper<IDbDataParameter, int[]?> ArrayBindSize { get; }

        public ObjectWrapper<IDbDataParameter, bool> IsNullable { get; }

        public ObjectEnumWrapper<IDbDataParameter, OracleMappingCollectionType> CollectionType { get; }

        public ObjectEnumWrapper<IDbDataParameter, OracleParameterMappingStatus> Status { get; }
    }

    private sealed class CommandExpressions
    {
        [SuppressMessage(
            "Design",
            "CC0021:Use nameof",
            Justification = "Refers to external ODP.NET namespace prefix, not AdaskoTheBeAsT.Dapper.Oracle.")]
        [SuppressMessage(
            "Style",
            "CC0021:Use nameof",
            Justification = "Refers to external ODP.NET namespace prefix, not AdaskoTheBeAsT.Dapper.Oracle.")]
        public CommandExpressions(Type commandType)
        {
            var ns = commandType.Namespace ?? string.Empty;
            if (!ns.StartsWith("Oracle", StringComparison.Ordinal)
                && !ns.StartsWith("Castle", StringComparison.Ordinal))
            {
                throw new NotSupportedException(
                    $"Whoopsies! This library will only work with Oracle types, you are attempting to use type {commandType.FullName}, which is not supported.");
            }

            BindByName = new ObjectWrapper<IDbCommand, bool>(nameof(BindByName), commandType);
            InitialLOBFetchSize = new ObjectWrapper<IDbCommand, int>(nameof(InitialLOBFetchSize), commandType);
            InitialLONGFetchSize = new ObjectWrapper<IDbCommand, int>(nameof(InitialLONGFetchSize), commandType);
            ArrayBindCount = new ObjectWrapper<IDbCommand, int>(nameof(ArrayBindCount), commandType);
            FetchSize = new ObjectWrapper<IDbCommand, long>(nameof(FetchSize), commandType);
            AddToStatementCache = new ObjectWrapper<IDbCommand, bool>(nameof(AddToStatementCache), commandType);
        }

        public ObjectWrapper<IDbCommand, bool> BindByName { get; }

        public ObjectWrapper<IDbCommand, int> InitialLOBFetchSize { get; }

        public ObjectWrapper<IDbCommand, int> InitialLONGFetchSize { get; }

        public ObjectWrapper<IDbCommand, int> ArrayBindCount { get; }

        public ObjectWrapper<IDbCommand, long> FetchSize { get; }

        public ObjectWrapper<IDbCommand, bool> AddToStatementCache { get; }
    }
}
