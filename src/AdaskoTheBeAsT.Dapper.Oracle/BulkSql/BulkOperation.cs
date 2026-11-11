using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Dapper.Oracle.Util;
using Dapper;

namespace AdaskoTheBeAsT.Dapper.Oracle.BulkSql;

public static class BulkOperation
{
    public static int SqlBulk<T>(
        this IDbConnection connection,
        string sql,
        IEnumerable<T> objects,
        IEnumerable<BulkMapping<T>> mapping,
        IDbTransaction? transaction = null,
        CommandType? cmdType = CommandType.Text)
    {
        return SqlBulk(connection, sql, objects, mapping, out _, cmdType, transaction);
    }

    public static int SqlBulk<T>(
        this IDbConnection connection,
        string sql,
        IEnumerable<T> objects,
        IEnumerable<BulkMapping<T>> mapping,
        out OracleDynamicParameters parameters,
        CommandType? cmdType = CommandType.Text,
        IDbTransaction? transaction = null)
    {
        ThrowHelper.ThrowIfNull(connection, nameof(connection));

        parameters = CreateParameterFromObject(objects, mapping);
        if (parameters.ArrayBindCount == 0)
        {
            return 0;
        }

        return connection.Execute(sql, parameters, transaction, commandType: cmdType);
    }

    public static async Task<AsyncQueryResult> SqlBulkAsync<T>(
        this IDbConnection connection,
        string sql,
        IEnumerable<T> objects,
        IEnumerable<BulkMapping<T>> mapping,
        CommandType? cmdType = CommandType.Text,
        IDbTransaction? transaction = null)
    {
        ThrowHelper.ThrowIfNull(connection, nameof(connection));

        var parameters = CreateParameterFromObject(objects, mapping);
        if (parameters.ArrayBindCount == 0)
        {
            return new AsyncQueryResult
            {
                ExecuteResult = 0,
                Parameters = parameters,
            };
        }

        var result = await connection.ExecuteAsync(sql, parameters, transaction, commandType: cmdType)
            .ConfigureAwait(false);
        return new AsyncQueryResult
        {
            ExecuteResult = result,
            Parameters = parameters,
        };
    }

    internal static OracleDynamicParameters CreateParameterFromObject<T>(
        IEnumerable<T> objects,
        IEnumerable<BulkMapping<T>> mapping)
    {
        ThrowHelper.ThrowIfNull(objects, nameof(objects));
        ThrowHelper.ThrowIfNull(mapping, nameof(mapping));

        var parameters = new OracleDynamicParameters();
        var items = objects.ToList();
        parameters.ArrayBindCount = items.Count;
        parameters.BindByName = true;
        foreach (var map in mapping)
        {
            var rawValues = map.Property != null ? items.Select(map.Property).ToArray() : null;
            var firstNonNull = rawValues?.FirstOrDefault(static v => v != null);
            var dbType = map.DbType ?? (firstNonNull != null ? OracleMapper.GuessType(firstNonNull.GetType()) : null);
            var values = rawValues?.Select(v => OracleMapper.NormalizeBulkValue(v, dbType)).ToArray();

            parameters.Add(
                Clean(map.Name),
                values,
                dbType,
                map.ParameterDirection,
                map.Size,
                map.IsNullable,
                map.Precision,
                map.Scale,
                map.SourceColumn,
                map.SourceVersion,
                map.CollectionType,
                map.ArrayBindSize);
        }

        return parameters;
    }

    private static string Clean(string name)
    {
        if (!string.IsNullOrEmpty(name)
            && (name[0] == '@' || name[0] == ':'))
        {
            return name.Substring(1);
        }

        return name;
    }
}
