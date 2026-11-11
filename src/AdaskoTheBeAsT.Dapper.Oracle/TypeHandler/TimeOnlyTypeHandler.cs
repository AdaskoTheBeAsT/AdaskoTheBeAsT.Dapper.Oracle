#if NET6_0_OR_GREATER
using System;
using System.Data;
using System.Globalization;
using AdaskoTheBeAsT.Dapper.Oracle.Util;

namespace AdaskoTheBeAsT.Dapper.Oracle.TypeHandler;

/// <summary>
/// Conversion between <see cref="TimeOnly"/> and Oracle <c>INTERVAL DAY TO SECOND</c> (or <c>DATE</c>/<c>TIMESTAMP</c>) data type.
/// </summary>
public class TimeOnlyTypeHandler : TypeHandlerBase<TimeOnly>
{
    public override void SetValue(
        IDbDataParameter parameter,
        TimeOnly value)
    {
        ThrowHelper.ThrowIfNull(parameter, nameof(parameter));

        SetOracleDbTypeOnParameter(parameter, "IntervalDS");
        parameter.Value = value.ToTimeSpan();
    }

    public override TimeOnly Parse(object value)
    {
        ThrowHelper.ThrowIfNull(value, nameof(value));

        if (value is TimeOnly timeOnly)
        {
            return timeOnly;
        }

        if (value is TimeSpan timeSpan)
        {
            return TimeOnly.FromTimeSpan(timeSpan);
        }

        if (value is DateTime dateTime)
        {
            return TimeOnly.FromDateTime(dateTime);
        }

        if (value is string stringValue)
        {
            return TimeOnly.Parse(stringValue, CultureInfo.InvariantCulture);
        }

        var converted = OracleValueConverter.Convert<TimeOnly?>(value);
        if (converted.HasValue)
        {
            return converted.Value;
        }

        throw new NotSupportedException($"Dont know how to convert a {value.GetType().Name} to a System.TimeOnly.");
    }
}
#endif
