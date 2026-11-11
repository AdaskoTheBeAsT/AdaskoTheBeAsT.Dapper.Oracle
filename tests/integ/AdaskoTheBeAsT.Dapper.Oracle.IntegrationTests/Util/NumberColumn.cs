using System.Globalization;

namespace AdaskoTheBeAsT.Dapper.Oracle.IntegrationTests.Util;

public class NumberColumn : TableColumn
{
    public override string ToString()
    {
        return string.Create(CultureInfo.InvariantCulture, $"{Name} NUMBER{GetSize()} {GetNullable(Nullable)}");
    }
}
