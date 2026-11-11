using System;
using System.Collections.Concurrent;
using System.Data;
using System.Linq.Expressions;
using AdaskoTheBeAsT.Dapper.Oracle.Util;

namespace AdaskoTheBeAsT.Dapper.Oracle.TypeHandler;

internal static class TypeHandlerCache
{
    private static readonly ConcurrentDictionary<DictionaryKey, Action<IDbDataParameter>> OracleDbTypeProperty = new();

    public static void SetOracleDbTypeOnParameter(
        IDbDataParameter parameter,
        string oracleTypeName,
        int? length = null)
    {
        ThrowHelper.ThrowIfNull(parameter, nameof(parameter));
        ThrowHelper.ThrowIfNull(oracleTypeName, nameof(oracleTypeName));

        var setter = OracleDbTypeProperty.GetOrAdd(
            new DictionaryKey(parameter.GetType(), oracleTypeName),
            CreateSetTypeAction);
        setter(parameter);
        if (length.HasValue)
        {
            parameter.Size = length.Value;
        }
    }

    private static Action<IDbDataParameter> CreateSetTypeAction(DictionaryKey key)
    {
        var enumType = key.ParameterType.Assembly.GetType($"{key.ParameterType.Namespace}.OracleDbType")
                       ?? throw new InvalidOperationException(
                           $"Enum type '{key.ParameterType.Namespace}.OracleDbType' was not found.");
        var enumValue = Enum.Parse(enumType, key.OracleTypeName);
        var inputVariable = Expression.Parameter(typeof(IDbDataParameter));
        var convertExpression = Expression.Convert(inputVariable, key.ParameterType);

        var expression = Expression.Assign(
            Expression.PropertyOrField(convertExpression, "OracleDbType"),
            Expression.Constant(enumValue));

        return Expression.Lambda<Action<IDbDataParameter>>(expression, inputVariable).Compile();
    }

    private readonly struct DictionaryKey : IEquatable<DictionaryKey>
    {
        public DictionaryKey(
            Type parameterType,
            string oracleTypeName)
        {
            ParameterType = parameterType;
            OracleTypeName = oracleTypeName;
        }

        public Type ParameterType { get; }

        public string OracleTypeName { get; }

        public bool Equals(DictionaryKey other)
        {
            return ParameterType == other.ParameterType
                   && string.Equals(OracleTypeName, other.OracleTypeName, StringComparison.Ordinal);
        }

        public override bool Equals(object? obj)
        {
            return obj is DictionaryKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (ParameterType.GetHashCode() * 397) ^ StringComparer.Ordinal.GetHashCode(OracleTypeName);
            }
        }
    }
}
