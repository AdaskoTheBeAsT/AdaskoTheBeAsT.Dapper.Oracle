using System;
using System.Data;
using AdaskoTheBeAsT.Dapper.Oracle.Util;

namespace AdaskoTheBeAsT.Dapper.Oracle;

internal static class CommandExtensions
{
    public static bool IsWrapped(this IDbConnection connection)
    {
        ThrowHelper.ThrowIfNull(connection, nameof(connection));

        return !string.Equals(connection.GetType().Name, "OracleConnection", StringComparison.Ordinal);
    }

    public static bool IsWrapped(this IDbCommand command)
    {
        ThrowHelper.ThrowIfNull(command, nameof(command));

        return !string.Equals(command.GetType().Name, "OracleCommand", StringComparison.Ordinal);
    }
}
