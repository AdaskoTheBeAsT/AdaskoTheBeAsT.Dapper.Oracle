using System.Globalization;

namespace AdaskoTheBeAsT.Dapper.Oracle.IntegrationTests.Util;

public class TableColumn
{
    public string Name { get; set; } = string.Empty;

    public OracleMappingType DataType { get; set; }

    public int Size { get; set; }

    public bool Nullable { get; set; }

    public bool PrimaryKey { get; set; }

    public override string ToString()
    {
        return string.Create(CultureInfo.InvariantCulture, $"{Name} {DataType}{GetSize()} {GetNullable(Nullable)}");
    }

    protected static string GetNullable(bool nullable)
    {
        return nullable ? string.Empty : "NOT NULL";
    }

    protected string GetSize()
    {
        if (Size == 0)
        {
            return string.Empty;
        }

        return string.Create(CultureInfo.InvariantCulture, $"({Size})");
    }
}
