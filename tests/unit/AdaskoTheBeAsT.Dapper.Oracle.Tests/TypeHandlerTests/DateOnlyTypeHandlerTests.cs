using System;
using AdaskoTheBeAsT.Dapper.Oracle.TypeHandler;
using AwesomeAssertions;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.Oracle.Tests.TypeHandlerTests;

public class DateOnlyTypeHandlerTests
{
    [Fact]
    public void SetValue_SetsOracleDbTypeDateWithMidnightDateTime()
    {
        var input = new DateOnly(2026, 4, 15);
        using var parameter = new OracleParameter();
        var sut = new DateOnlyTypeHandler();

        sut.SetValue(parameter, input);

        parameter.OracleDbType.Should().Be(OracleDbType.Date);
        parameter.Value.Should().Be(new DateTime(2026, 4, 15, 0, 0, 0, DateTimeKind.Unspecified));
    }

    [Fact]
    public void Parse_SupportsDateOnlyDateTimeOracleDateAndString()
    {
        var expected = new DateOnly(2026, 4, 15);
        var sut = new DateOnlyTypeHandler();

        sut.Parse(expected).Should().Be(expected);
        sut.Parse(new DateTime(2026, 4, 15, 13, 45, 0, DateTimeKind.Unspecified)).Should().Be(expected);
        sut.Parse(new OracleDate(2026, 4, 15)).Should().Be(expected);
        sut.Parse("2026-04-15").Should().Be(expected);
    }
}
