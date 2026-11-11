using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using Dapper;

namespace AdaskoTheBeAsT.Dapper.Oracle.IntegrationTests.Util;

public static class TableCreator
{
    public static void Create(
        IDbConnection connection,
        string tableName,
        IEnumerable<TableColumn> columns)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(columns);

        var dropSql = """
                      BEGIN
                      EXECUTE IMMEDIATE 'DROP TABLE {tableName}';
                          EXCEPTION
                               WHEN OTHERS THEN
                                      IF SQLCODE != -942 THEN
                                           RAISE;
                                      END IF;
                          END;
                      """.Replace("{tableName}", tableName, StringComparison.Ordinal);

        connection.Execute(dropSql.Replace("\r\n", "\n", StringComparison.Ordinal));

        var list = columns.ToArray();
        var sb = new StringBuilder();

        sb.AppendLine(CultureInfo.InvariantCulture, $"CREATE TABLE {tableName}(");
        sb.AppendJoin(",\n", list.Select(static c => c.ToString())).AppendLine();

        if (Array.Exists(list, static l => l.PrimaryKey))
        {
            var pkColumns = string.Join(',', list.Where(static l => l.PrimaryKey).Select(static c => c.Name));
            sb.AppendLine(CultureInfo.InvariantCulture, $",CONSTRAINT pk_{tableName} PRIMARY KEY ({pkColumns}))");
        }
        else
        {
            sb.AppendLine(")");
        }

        connection.Execute(sb.ToString().Replace("\r\n", "\n", StringComparison.Ordinal));
    }
}
