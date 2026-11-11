using System;
using AdaskoTheBeAsT.Dapper.Oracle.TypeHandler;
using AwesomeAssertions;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.Oracle.Tests.TypeHandlerTests;

public class TimeOnlyTypeHandlerTests
{
    [Fact]
    public void SetValue_SetsOracleDbTypeIntervalDsWithTimeSpan()
    {
        var input = new TimeOnly(14, 30, 45, 500);
        using var parameter = new OracleParameter();
        var sut = new TimeOnlyTypeHandler();

        sut.SetValue(parameter, input);

        parameter.OracleDbType.Should().Be(OracleDbType.IntervalDS);
        parameter.Value.Should().Be(input.ToTimeSpan());
    }

    [Fact]
    public void Parse_SupportsTimeOnlyTimeSpanDateTimeOracleIntervalDsAndString()
    {
        var expected = new TimeOnly(14, 30, 45);
        var sut = new TimeOnlyTypeHandler();

        sut.Parse(expected).Should().Be(expected);
        sut.Parse(new TimeSpan(14, 30, 45)).Should().Be(expected);
        sut.Parse(new DateTime(2026, 4, 15, 14, 30, 45, DateTimeKind.Unspecified)).Should().Be(expected);
        sut.Parse(new OracleIntervalDS(0, 14, 30, 45, 0)).Should().Be(expected);
        sut.Parse("14:30:45").Should().Be(expected);
    }
}
