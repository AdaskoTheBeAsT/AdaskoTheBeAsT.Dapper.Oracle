using System.Data;

namespace AdaskoTheBeAsT.Dapper.Oracle.Util;

internal static class ConnectionExtensions
{
    public static T DownCastConnection<T>(this T connection)
        where T : IDbConnection
    {
        return DownCaster<T>.DownCast(connection);
    }

    public static T DownCastCommand<T>(this T command)
        where T : IDbCommand
    {
        return DownCaster<T>.DownCast(command);
    }
}
