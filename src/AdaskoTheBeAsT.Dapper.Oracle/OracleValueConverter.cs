using System;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AdaskoTheBeAsT.Dapper.Oracle;

[SuppressMessage(
    "Performance",
    "SYSLIB1045:Convert to 'GeneratedRegexAttribute'.",
    Justification =
        "GeneratedRegex triggers AD0001 in IDisposableAnalyzers and is unavailable on netstandard2.0/net461.")]
[SuppressMessage(
    "ReflectionAnalyzers.SystemReflection",
    "REFL009:The referenced member is not known to exist",
    Justification = "Accesses Oracle ODP.NET value-type members dynamically without hard assembly reference.")]
internal static class OracleValueConverter
{
    private static readonly Regex OracleTypeRegex = new(
        @"Oracle\.\w+\.Types\.Oracle\w+",
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1));

    public static T Convert<T>(object? value)
    {
        var unwrapped = UnwrapParameter(value);
        if (unwrapped == null
            || unwrapped == DBNull.Value)
        {
            return default!;
        }

        var rawType = unwrapped.GetType();
        var isOracleStructure = IsOracleDataStructure(rawType);
        if (isOracleStructure
            && IsOracleNull(unwrapped, rawType))
        {
            return default!;
        }

        if (unwrapped is T directRawMatch)
        {
            return directRawMatch;
        }

        var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
        if (isOracleStructure
            && targetType == typeof(DateTimeOffset)
            && TryConvertOracleTimeStampTz(unwrapped, rawType, out var dto))
        {
            return (T)(object)dto;
        }

        var extracted = isOracleStructure
            ? rawType.GetProperty("Value")?.GetValue(unwrapped) ?? unwrapped
            : unwrapped;

        return ConvertExtracted<T>(extracted, targetType);
    }

    private static T ConvertExtracted<T>(
        object? extracted,
        Type targetType)
    {
        if (extracted == null
            || extracted == DBNull.Value)
        {
            return default!;
        }

        if (extracted is T directMatch)
        {
            return directMatch;
        }

        if (TryConvertSpecialClrTypes(extracted, targetType, out var specialConverted))
        {
            return (T)specialConverted!;
        }

        if (extracted is IConvertible)
        {
            return (T)System.Convert.ChangeType(extracted, targetType, CultureInfo.InvariantCulture);
        }

        if (typeof(T).BaseType == typeof(Array)
            || extracted.GetType().BaseType == typeof(Array))
        {
            var convertedArray = ConvertArray<T>(extracted, targetType);
            if (convertedArray is T typedArray)
            {
                return typedArray;
            }

            return (T)System.Convert.ChangeType(convertedArray, targetType, CultureInfo.InvariantCulture);
        }

        return default!;
    }

    private static object ConvertArray<T>(
        object value,
        Type targetType)
    {
        var arr = (Array)value;

        if (typeof(T) == typeof(DateTime[]))
        {
            return ConvertDateTimeArray(arr);
        }

        if (typeof(T) == typeof(DateTime?[]))
        {
            return ConvertNullableDateTimeArray(arr);
        }

        return typeof(T).FullName switch
        {
            "Oracle.ManagedDataAccess.Types.OracleString[]" or "System.Byte[]" =>
                System.Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture),
            "System.Int16[]" =>
                ParseArray(arr, static s => short.Parse(s, CultureInfo.InvariantCulture)),
            "System.Int32[]" =>
                ParseArray(arr, static s => int.Parse(s, CultureInfo.InvariantCulture)),
            "System.Int64[]" =>
                ParseArray(arr, static s => long.Parse(s, CultureInfo.InvariantCulture)),
            "System.Single[]" =>
                ParseArray(arr, static s => float.Parse(s, CultureInfo.InvariantCulture)),
            "System.Double[]" =>
                ParseArray(arr, static s => double.Parse(s, CultureInfo.InvariantCulture)),
            "System.Decimal[]" or "Oracle.ManagedDataAccess.Types.OracleDecimal[]" =>
                ParseArray(arr, static s => decimal.Parse(s, CultureInfo.InvariantCulture)),
            "System.Boolean[]" =>
                ParseArray(arr, bool.Parse),
            _ => ConvertStringArray(arr),
        };
    }

    private static DateTime[] ConvertDateTimeArray(Array arr)
    {
        var dateArray = new DateTime[arr.Length];
        for (var i = 0; i < arr.Length; i++)
        {
            var oraDate = arr.GetValue(i)
                          ?? throw new InvalidOperationException(
                              "Cannot convert null OracleDate element to non-nullable DateTime.");
            var valueProp = oraDate.GetType().GetProperty("Value")
                            ?? throw new InvalidOperationException(
                                $"Property 'Value' was not found on '{oraDate.GetType().FullName}'.");
            dateArray[i] = (DateTime)valueProp.GetValue(oraDate, index: null)!;
        }

        return dateArray;
    }

    private static DateTime?[] ConvertNullableDateTimeArray(Array arr)
    {
        var dateArray = new DateTime?[arr.Length];
        for (var i = 0; i < arr.Length; i++)
        {
            var oraDate = arr.GetValue(i);
            if (oraDate == null)
            {
                dateArray[i] = null;
                continue;
            }

            var oraType = oraDate.GetType();
            var isNullValue = oraType.GetProperty("IsNull")?.GetValue(oraDate, index: null);
            if (isNullValue is false)
            {
                dateArray[i] = (DateTime?)oraType.GetProperty("Value")?.GetValue(oraDate, index: null);
            }
        }

        return dateArray;
    }

    private static TElement[] ParseArray<TElement>(
        Array arr,
        Func<string, TElement> parser)
    {
        var result = new TElement[arr.Length];
        if (parser == null)
        {
            return result;
        }

        for (var i = 0; i < arr.Length; i++)
        {
            var text = arr.GetValue(i)?.ToString()
                       ?? throw new InvalidOperationException(
                           $"Array element at index {i.ToString(CultureInfo.InvariantCulture)} cannot be null.");
            result[i] = parser(text);
        }

        return result;
    }

    private static string?[] ConvertStringArray(Array arr)
    {
        var strArray = new string?[arr.Length];
        for (var i = 0; i < arr.Length; i++)
        {
            strArray[i] = arr.GetValue(i)?.ToString();
        }

        return strArray;
    }

    private static object? UnwrapParameter(object? value)
    {
        while (value is DbParameter parameter)
        {
            value = parameter.Value;
        }

        return value;
    }

    private static bool IsOracleNull(
        object value,
        Type valueType)
    {
        var isNullProp = valueType.GetProperty("IsNull", typeof(bool));
        return isNullProp?.GetValue(value) is true;
    }

    private static bool TryConvertOracleTimeStampTz(
        object value,
        Type valueType,
        out DateTimeOffset result)
    {
        if (!string.Equals(valueType.Name, "OracleTimeStampTZ", StringComparison.Ordinal))
        {
            result = default;
            return false;
        }

        var dateValue = valueType.GetProperty("Value")?.GetValue(value);
        var offsetMethod = valueType.GetMethod("GetTimeZoneOffset", Type.EmptyTypes);
        if (dateValue is DateTime dt
            && offsetMethod?.Invoke(value, parameters: null) is TimeSpan offset)
        {
            result = new DateTimeOffset(DateTime.SpecifyKind(dt, DateTimeKind.Unspecified), offset);
            return true;
        }

        result = default;
        return false;
    }

    private static bool TryConvertSpecialClrTypes(
        object extracted,
        Type targetType,
        out object? converted)
    {
        if (targetType == typeof(Guid)
            && extracted is byte[] guidBytes
            && guidBytes.Length == 16)
        {
            converted = new Guid(guidBytes);
            return true;
        }

#if NET6_0_OR_GREATER
        if (targetType == typeof(DateOnly)
            && extracted is DateTime dateValue)
        {
            converted = DateOnly.FromDateTime(dateValue);
            return true;
        }

        if (targetType == typeof(TimeOnly))
        {
            if (extracted is TimeSpan timeSpanValue)
            {
                converted = TimeOnly.FromTimeSpan(timeSpanValue);
                return true;
            }

            if (extracted is DateTime dateTimeValue)
            {
                converted = TimeOnly.FromDateTime(dateTimeValue);
                return true;
            }
        }
#endif

        converted = null;
        return false;
    }

    private static bool IsOracleDataStructure(Type valueType)
    {
        var fullName = valueType.FullName;
        if (string.IsNullOrEmpty(fullName))
        {
            return false;
        }

        return OracleTypeRegex.IsMatch(fullName);
    }
}
