using System;
using AdaskoTheBeAsT.Dapper.Oracle.Util;
using Oracle.ManagedDataAccess.Client;

namespace AdaskoTheBeAsT.Dapper.Oracle.Tests;

public class OracleManagedParameterRetretreiver : IOracleParameterRetretreiver
{
    public OracleParameterData GetParameter(object parameter)
    {
        ThrowHelper.ThrowIfNull(parameter, nameof(parameter));

        var oraParam = (OracleParameter)parameter;
        return new OracleParameterData
        {
            ParameterName = oraParam.ParameterName,
#if NET5_0_OR_GREATER
            OracleDbType = Enum.GetName(oraParam.OracleDbType),
            CollectionType = Enum.GetName(oraParam.CollectionType),
#else
            OracleDbType = Enum.GetName(typeof(OracleDbType), oraParam.OracleDbType),
            CollectionType = Enum.GetName(typeof(OracleCollectionType), oraParam.CollectionType),
#endif
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
