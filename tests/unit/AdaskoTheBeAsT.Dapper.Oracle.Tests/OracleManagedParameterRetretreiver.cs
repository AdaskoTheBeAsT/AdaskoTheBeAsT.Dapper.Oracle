using System;
using Oracle.ManagedDataAccess.Client;

namespace AdaskoTheBeAsT.Dapper.Oracle.Tests;

public class OracleManagedParameterRetretreiver : IOracleParameterRetretreiver
{
    public OracleParameterData GetParameter(object parameter)
    {
        ArgumentNullException.ThrowIfNull(parameter);

        var oraParam = (OracleParameter)parameter;
        return new OracleParameterData
        {
            ParameterName = oraParam.ParameterName,
            OracleDbType = Enum.GetName(oraParam.OracleDbType),
            CollectionType = Enum.GetName(oraParam.CollectionType),
            Value = oraParam.Value,
            Direction = oraParam.Direction,
            Size = oraParam.Size,
            IsNullable = oraParam.IsNullable,
            Precision = oraParam.Precision,
            SourceColumn = oraParam.SourceColumn,
            SourceVersion = oraParam.SourceVersion,
            ArrayBindSize = oraParam.ArrayBindSize,
        };
    }
}
