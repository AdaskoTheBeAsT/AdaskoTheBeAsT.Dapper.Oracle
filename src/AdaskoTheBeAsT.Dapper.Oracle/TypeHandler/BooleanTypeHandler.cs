using System;
using System.Data;
using System.Globalization;
using AdaskoTheBeAsT.Dapper.Oracle.Util;

namespace AdaskoTheBeAsT.Dapper.Oracle.TypeHandler;

/// <summary>
/// Conversion between <see cref="bool"/> and Oracle Database 23ai native SQL <c>BOOLEAN</c> (or PL/SQL <c>BOOLEAN</c>) data type.
/// </summary>
public class BooleanTypeHandler : TypeHandlerBase<bool>
{
    public override void SetValue(
        IDbDataParameter parameter,
        bool value)
    {
        ThrowHelper.ThrowIfNull(parameter, nameof(parameter));

        SetOracleDbTypeOnParameter(parameter, "Boolean");
        parameter.Value = value;
    }

    public override bool Parse(object value)
    {
        ThrowHelper.ThrowIfNull(value, nameof(value));

        if (value is bool boolValue)
        {
            return boolValue;
        }

        if (value is IConvertible)
        {
            return Convert.ToBoolean(value, CultureInfo.InvariantCulture);
        }

        return OracleValueConverter.Convert<bool>(value);
    }
}
