using System.Data;

namespace AdaskoTheBeAsT.Dapper.Oracle.Tests;

internal sealed class TestableOracleDynamicParameters : OracleDynamicParameters
{
    public void AddParam(IDbCommand command)
    {
        AddParameters(command, identity: null);
    }
}
