using System.Data;
using System.Diagnostics.CodeAnalysis;
using Dapper;

namespace AdaskoTheBeAsT.Dapper.Oracle.TypeHandler;

public abstract class TypeHandlerBase<T> : SqlMapper.TypeHandler<T>
{
    [SuppressMessage(
        "Performance",
        "CA1822:Mark members as static",
        Justification = "Preserves protected instance API for derived type handlers.")]
    [SuppressMessage(
        "Design",
        "CC0091:Use static method",
        Justification = "Preserves protected instance API for derived type handlers.")]
    [SuppressMessage(
        "Minor Code Smell",
        "S2325:Methods and properties that don't access instance data should be static",
        Justification = "Preserves protected instance API for derived type handlers.")]
    protected void SetOracleDbTypeOnParameter(
        IDbDataParameter parameter,
        string oracleTypeName,
        int? length = null)
    {
        TypeHandlerCache.SetOracleDbTypeOnParameter(parameter, oracleTypeName, length);
    }
}
