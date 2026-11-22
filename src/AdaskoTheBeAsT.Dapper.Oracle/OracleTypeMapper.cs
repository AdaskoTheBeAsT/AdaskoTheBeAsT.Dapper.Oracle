using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using AdaskoTheBeAsT.Dapper.Oracle.Util;
using Dapper;

namespace AdaskoTheBeAsT.Dapper.Oracle;

public static class OracleTypeMapper
{
    internal static readonly ConcurrentDictionary<Type, SqlMapper.ITypeHandler> Handlers = new();

    public static void AddTypeHandler(
        Type type,
        SqlMapper.ITypeHandler handler)
    {
        ThrowHelper.ThrowIfNull(type, nameof(type));
        ThrowHelper.ThrowIfNull(handler, nameof(handler));

        Handlers[type] = handler;
        SqlMapper.AddTypeHandler(type, handler);
    }

    public static void AddTypeHandler<T>(SqlMapper.TypeHandler<T> handler)
    {
        ThrowHelper.ThrowIfNull(handler, nameof(handler));

        Handlers[typeof(T)] = handler;
        SqlMapper.AddTypeHandler(handler);
    }

    public static void AddTypeHandler<T>(SqlMapper.ITypeHandler handler)
    {
        ThrowHelper.ThrowIfNull(handler, nameof(handler));

        Handlers[typeof(T)] = handler;
        SqlMapper.AddTypeHandler(typeof(T), handler);
    }

    public static void AddTypeHandler<TType, THandler>()
        where THandler : SqlMapper.TypeHandler<TType>, new()
    {
        var handler = new THandler();
        Handlers[typeof(TType)] = handler;
        SqlMapper.AddTypeHandler(handler);
    }

    public static bool HasTypeHandler(
        Type type,
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        [NotNullWhen(true)]
#endif
        out SqlMapper.ITypeHandler? handler)
    {
        ThrowHelper.ThrowIfNull(type, nameof(type));

        return Handlers.TryGetValue(type, out handler);
    }
}
