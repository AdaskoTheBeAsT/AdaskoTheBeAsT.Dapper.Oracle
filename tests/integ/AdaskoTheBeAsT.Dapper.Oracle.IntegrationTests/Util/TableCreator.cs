using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using AdaskoTheBeAsT.Dapper.Oracle.Util;
using Dapper;

namespace AdaskoTheBeAsT.Dapper.Oracle.IntegrationTests.Util;

public static class TableCreator
{
    public static void Create(
        IDbConnection connection,
        string tableName,
        IEnumerable<TableColumn> columns)
    {
        ThrowHelper.ThrowIfNull(connection, nameof(connection));
        ThrowHelper.ThrowIfNull(tableName, nameof(tableName));
        ThrowHelper.ThrowIfNull(columns, nameof(columns));

        var dropSql = """
                      BEGIN
                      EXECUTE IMMEDIATE 'DROP TABLE {tableName}';
                          EXCEPTION
                               WHEN OTHERS THEN
                                      IF SQLCODE != -942 THEN
                                           RAISE;
                                      END IF;
                          END;
                      """.ReplaceOrdinal("{tableName}", tableName);

        connection.Execute(dropSql.ReplaceOrdinal("\r\n", "\n"));

        var list = columns.ToArray();
        var sb = new StringBuilder();

        sb.Append("CREATE TABLE ").Append(tableName).AppendLine("(");
#if NETCOREAPP3_0_OR_GREATER
        sb.AppendJoin(",\n", list.Select(static c => c.ToString())).AppendLine();
#else
        sb.AppendLine(string.Join(",\n", list.Select(static c => c.ToString())));
#endif

        if (Array.Exists(list, static l => l.PrimaryKey))
        {
#if NETCOREAPP3_0_OR_GREATER
            var pkColumns = string.Join(',', list.Where(static l => l.PrimaryKey).Select(static c => c.Name));
#else
            var pkColumns = string.Join(",", list.Where(static l => l.PrimaryKey).Select(static c => c.Name));
#endif
            sb.Append(",CONSTRAINT pk_").Append(tableName).Append(" PRIMARY KEY (").Append(pkColumns).AppendLine("))");
        }
        else
        {
            sb.AppendLine(")");
        }

        connection.Execute(sb.ToString().ReplaceOrdinal("\r\n", "\n"));
    }
}
