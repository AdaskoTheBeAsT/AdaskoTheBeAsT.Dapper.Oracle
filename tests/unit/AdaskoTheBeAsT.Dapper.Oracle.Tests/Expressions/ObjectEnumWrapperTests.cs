using System;
using System.Data;
using AdaskoTheBeAsT.Dapper.Oracle.Expressions;
using AwesomeAssertions;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.Oracle.Tests.Expressions;

public class ObjectEnumWrapperTests : ParameterBaseTests
{
    [Theory]
    [MemberData(nameof(OracleDataFixture), DisableDiscoveryEnumeration = true)]
    public void BasicTest(IDbCommand cmd)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var param = cmd.CreateParameter();
        var setter = new ObjectEnumWrapper<IDbDataParameter, OracleMappingType>(
            "OracleDbType",
            "OracleDbType",
            param.GetType());
        setter.SetValue(param, OracleMappingType.Date);
        var value = setter.GetValue(param);
        value.ToString().Should().Be("Date");
    }
}
