#if NET6_0_OR_GREATER
using System;
using System.Data;
using System.Globalization;
using AdaskoTheBeAsT.Dapper.Oracle.Util;

namespace AdaskoTheBeAsT.Dapper.Oracle.TypeHandler;

/// <summary>
/// Conversion between <see cref="DateOnly"/> and Oracle <c>DATE</c> data type.
/// </summary>
public class DateOnlyTypeHandler : TypeHandlerBase<DateOnly>
{
    public override void SetValue(
        IDbDataParameter parameter,
        DateOnly value)
    {
        ThrowHelper.ThrowIfNull(parameter, nameof(parameter));

        SetOracleDbTypeOnParameter(parameter, "Date");
        parameter.Value = value.ToDateTime(TimeOnly.MinValue);
    }

    public override DateOnly Parse(object value)
    {
        ThrowHelper.ThrowIfNull(value, nameof(value));

        if (value is DateOnly dateOnly)
        {
            return dateOnly;
        }

        if (value is DateTime dateTime)
        {
            return DateOnly.FromDateTime(dateTime);
        }

        if (value is string stringValue)
        {
            return DateOnly.Parse(stringValue, CultureInfo.InvariantCulture);
        }

        var converted = OracleValueConverter.Convert<DateOnly?>(value);
        if (converted.HasValue)
        {
            return converted.Value;
        }

        throw new NotSupportedException($"Dont know how to convert a {value.GetType().Name} to a System.DateOnly.");
    }
}
#endif
