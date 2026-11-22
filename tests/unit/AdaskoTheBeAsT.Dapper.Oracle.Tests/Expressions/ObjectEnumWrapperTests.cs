using System;
using System.Data;
using AdaskoTheBeAsT.Dapper.Oracle.Expressions;
using AdaskoTheBeAsT.Dapper.Oracle.Util;
using AwesomeAssertions;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.Oracle.Tests.Expressions;

public class ObjectEnumWrapperTests : ParameterBaseTests
{
    [Theory]
    [MemberData(nameof(OracleDataFixture), DisableDiscoveryEnumeration = true)]
    public void BasicTest(IDbCommand cmd)
    {
        ThrowHelper.ThrowIfNull(cmd, nameof(cmd));

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
