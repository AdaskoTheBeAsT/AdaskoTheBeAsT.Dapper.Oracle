using AdaskoTheBeAsT.Dapper.Oracle.TypeHandler;
using AwesomeAssertions;
using Oracle.ManagedDataAccess.Client;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.Oracle.Tests.TypeHandlerTests;

public class BooleanNumericTypeHandlerTests
{
    [Fact]
    public void ConvertFromTrue()
    {
        using var parameter = new OracleParameter();
        var sut = new BooleanNumericTypeHandler();
        sut.SetValue(parameter, value: true);

        parameter.Value.Should().Be(1);
        parameter.OracleDbType.Should().Be(OracleDbType.Int16);
    }

    [Fact]
    public void ConvertToTrue()
    {
        var sut = new BooleanNumericTypeHandler();
        var result = sut.Parse(1);

        result.Should().BeTrue();
    }

    [Fact]
    public void ConvertFromFalse()
    {
        using var parameter = new OracleParameter();
        var sut = new BooleanNumericTypeHandler();
        sut.SetValue(parameter, value: false);

        parameter.Value.Should().Be(0);
        parameter.OracleDbType.Should().Be(OracleDbType.Int16);
    }

    [Theory]
    [InlineData((short)1, true)]
    [InlineData((short)0, false)]
    [InlineData((byte)1, true)]
    [InlineData((byte)0, false)]
    [InlineData(1L, true)]
    [InlineData(0L, false)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void Parse_SupportsAllNumericAndBooleanTypesReturnedByOdpNet(
        object rawValue,
        bool expected)
    {
        var sut = new BooleanNumericTypeHandler();

        sut.Parse(rawValue).Should().Be(expected);
    }

    [Fact]
    public void Parse_SupportsDecimalReturnedByUnconstrainedOracleNumber()
    {
        var sut = new BooleanNumericTypeHandler();

        sut.Parse(1m).Should().BeTrue();
        sut.Parse(0m).Should().BeFalse();
    }
}
