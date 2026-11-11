using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace AdaskoTheBeAsT.Dapper.Oracle.Util;

internal static class DownCaster<T>
{
    private static readonly ConcurrentDictionary<Type, (bool HasInner, Func<T, T>? Getter)> Cache = new();

    internal static T DownCast(T item)
    {
        return GetInner(item);
    }

    private static T GetInner(T item)
    {
        if (EqualityComparer<T>.Default.Equals(item, default!))
        {
            return default!;
        }

        var type = item!.GetType();
        var info = Cache.GetOrAdd(type, CreateAccessor);
        if (!info.HasInner
            || info.Getter == null)
        {
            return item;
        }

        return GetInner(info.Getter(item));
    }

    [SuppressMessage(
        "Major Code Smell",
        "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields",
        Justification = "Unwraps decorated IDbConnection/IDbCommand instances whose inner property may be non-public.")]
    private static (bool HasInner, Func<T, T>? Getter) CreateAccessor(Type type)
    {
        var properties = type
            .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(static pi => pi.CanRead && typeof(T).IsAssignableFrom(pi.PropertyType))
            .ToArray();

        if (properties.Length > 1)
        {
            throw new InvalidOperationException($"More than one inner property of type {typeof(T)}");
        }

        if (properties.Length == 0)
        {
            return (false, null);
        }

        var property = properties[0];
        return (true, new PropertyWrapper<T, T>(property, type).CreateGetter());
    }
}
