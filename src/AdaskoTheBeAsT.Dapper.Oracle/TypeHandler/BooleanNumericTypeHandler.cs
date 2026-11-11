using System;
using System.Data;
using System.Globalization;
using AdaskoTheBeAsT.Dapper.Oracle.Util;

namespace AdaskoTheBeAsT.Dapper.Oracle.TypeHandler;

/// <summary>
/// Conversion between <see cref="bool"/> in .NET and <c>NUMBER(1)</c> Oracle data type.
/// Numeric value 0 is false, any other value equals true.
/// </summary>
public class BooleanNumericTypeHandler : TypeHandlerBase<bool>
{
    public override void SetValue(
        IDbDataParameter parameter,
        bool value)
    {
        ThrowHelper.ThrowIfNull(parameter, nameof(parameter));

        SetOracleDbTypeOnParameter(parameter, "Int16");
        parameter.Value = value ? 1 : 0;
    }

    public override bool Parse(object value)
    {
        ThrowHelper.ThrowIfNull(value, nameof(value));

        if (value is bool boolValue)
        {
            return boolValue;
        }

        return Convert.ToDecimal(value, CultureInfo.InvariantCulture) != 0m;
    }
}
