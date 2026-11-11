using AdaskoTheBeAsT.Dapper.Oracle.TypeHandler;
using AwesomeAssertions;
using Oracle.ManagedDataAccess.Client;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.Oracle.Tests.TypeHandlerTests;

public class BooleanStringTypeHandlerTests
{
    [Fact]
    public void ConvertToTrue()
    {
        using var parameter = new OracleParameter();
        var sut = new BooleanStringTypeHandler("YEP", "NOPE");
        sut.SetValue(parameter, value: true);

        parameter.Value.Should().Be("YEP");
        parameter.OracleDbType.Should().Be(OracleDbType.Varchar2);
    }

    [Fact]
    public void ConvertToFalse()
    {
        using var parameter = new OracleParameter();
        var sut = new BooleanStringTypeHandler("YEP", "NOPE");
        sut.SetValue(parameter, value: false);

        parameter.Value.Should().Be("NOPE");
        parameter.OracleDbType.Should().Be(OracleDbType.Varchar2);
    }

    [Fact]
    public void ConvertFromFalse()
    {
        var sut = new BooleanStringTypeHandler("YEP", "NOPE");
        var result = sut.Parse("NOPE");
        result.Should().BeFalse();
    }

    [Fact]
    public void ConvertFromTrue()
    {
        var sut = new BooleanStringTypeHandler("YEP", "NOPE");
        var result = sut.Parse("YEP");
        result.Should().BeTrue();
    }
}
