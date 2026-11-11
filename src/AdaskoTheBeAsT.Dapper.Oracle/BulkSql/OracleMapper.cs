using System;
using System.Collections.Generic;
using System.Linq;
using AdaskoTheBeAsT.Dapper.Oracle.Util;

namespace AdaskoTheBeAsT.Dapper.Oracle.BulkSql;

internal static class OracleMapper
{
    private static readonly Dictionary<Type, OracleMappingType> OracleMappings = new()
    {
        {
            typeof(Guid), OracleMappingType.Raw
        },
        {
            typeof(byte[]), OracleMappingType.Raw
        },
        {
            typeof(string), OracleMappingType.Varchar2
        },
        {
            typeof(short), OracleMappingType.Int16
        },
        {
            typeof(int), OracleMappingType.Int32
        },
        {
            typeof(long), OracleMappingType.Int64
        },
        {
            typeof(float), OracleMappingType.BinaryFloat
        },
        {
            typeof(double), OracleMappingType.BinaryDouble
        },
        {
            typeof(decimal), OracleMappingType.Decimal
        },
        {
            typeof(DateTime), OracleMappingType.Date
        },
        {
            typeof(DateTimeOffset), OracleMappingType.TimeStampTZ
        },
        {
            typeof(TimeSpan), OracleMappingType.IntervalDS
        },
        {
            typeof(bool), OracleMappingType.Int16
        },
        {
            typeof(float[]), OracleMappingType.VectorFloat32
        },
        {
            typeof(double[]), OracleMappingType.VectorFloat64
        },
#if NET6_0_OR_GREATER
        {
            typeof(DateOnly), OracleMappingType.Date
        },
        {
            typeof(TimeOnly), OracleMappingType.IntervalDS
        },
#endif
    };

    public static OracleMappingType? GuessType(Type type)
    {
        ThrowHelper.ThrowIfNull(type, nameof(type));

        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        return OracleMappings.TryGetValue(underlying, out var mapping) ? mapping : null;
    }

    public static object? NormalizeBulkValue(
        object? value,
        OracleMappingType? dbType)
    {
        if (value == null)
        {
            return null;
        }

        if (value is Guid guid)
        {
            return guid.ToByteArray();
        }

        if (value is bool boolValue
            && dbType != OracleMappingType.Boolean)
        {
            return boolValue ? 1 : 0;
        }

#if NET6_0_OR_GREATER
        if (value is DateOnly dateOnly)
        {
            return dateOnly.ToDateTime(TimeOnly.MinValue);
        }

        if (value is TimeOnly timeOnly)
        {
            return timeOnly.ToTimeSpan();
        }
#endif

        return value;
    }

    public static OracleDynamicParameters Create<T>(IEnumerable<T> objects)
    {
        ThrowHelper.ThrowIfNull(objects, nameof(objects));

        var type = typeof(T);
        var entities = objects.ToList();

        var odp = new OracleDynamicParameters
        {
            ArrayBindCount = entities.Count,
            BindByName = true,
        };

        foreach (var pi in type.GetProperties())
        {
            if (!pi.CanRead)
            {
                continue;
            }

            var parameterName = pi.Name;
            var dbType = GuessType(pi.PropertyType);
            var values = entities.Select(p => NormalizeBulkValue(pi.GetValue(p), dbType)).ToArray();
            odp.Add(parameterName, values, dbType);
        }

        return odp;
    }
}
