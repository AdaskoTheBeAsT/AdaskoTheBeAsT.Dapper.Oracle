using System;
using System.Data;
using AdaskoTheBeAsT.Dapper.Oracle.Util;

namespace AdaskoTheBeAsT.Dapper.Oracle.TypeHandler;

/// <summary>
/// Conversion between <see cref="bool"/> and Oracle VARCHAR2.
/// </summary>
public class BooleanStringTypeHandler : TypeHandlerBase<bool>
{
    private readonly string _trueString;
    private readonly string _falseString;
    private readonly StringComparison _compareMode;

    /// <summary>
    /// Initializes a new instance of the <see cref="BooleanStringTypeHandler"/> class.
    /// </summary>
    /// <param name="trueValue">Value to use in database for a boolean true value.</param>
    /// <param name="falseValue">Value to use in database for a boolean false value.</param>
    /// <param name="comparison">String comparison mode.</param>
    public BooleanStringTypeHandler(
        string trueValue,
        string falseValue,
        StringComparison comparison = StringComparison.Ordinal)
    {
        _trueString = trueValue ?? throw new ArgumentNullException(nameof(trueValue));
        _falseString = falseValue ?? throw new ArgumentNullException(nameof(falseValue));
        _compareMode = comparison;
    }

    public override void SetValue(
        IDbDataParameter parameter,
        bool value)
    {
        ThrowHelper.ThrowIfNull(parameter, nameof(parameter));

        SetOracleDbTypeOnParameter(parameter, "Varchar2");
        parameter.Value = value ? _trueString : _falseString;
    }

    public override bool Parse(object value)
    {
        ThrowHelper.ThrowIfNull(value, nameof(value));

        if (value is string valueString)
        {
            if (valueString.Equals(_trueString, _compareMode))
            {
                return true;
            }

            if (valueString.Equals(_falseString, _compareMode))
            {
                return false;
            }

            throw new NotSupportedException(
                $"'{valueString}' was unexpected - expected '{_trueString}' or '{_falseString}'");
        }

        throw new NotSupportedException($"Dont know how to convert a {value.GetType()} to a Boolean");
    }
}
