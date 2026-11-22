namespace AdaskoTheBeAsT.Dapper.Oracle.IntegrationTests.Util;

public class NumberColumn : TableColumn
{
    public override string ToString()
    {
        return $"{Name} NUMBER{GetSize()} {GetNullable(Nullable)}";
    }
}
