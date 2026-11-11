using AdaskoTheBeAsT.Dapper.Oracle.TypeHandler;
using AwesomeAssertions;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.Oracle.Tests.TypeHandlerTests;

public class BooleanTypeHandlerTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetValue_SetsOracleDbTypeBoolean(bool input)
    {
        using var parameter = new OracleParameter();
        var sut = new BooleanTypeHandler();

        sut.SetValue(parameter, input);

        parameter.OracleDbType.Should().Be(OracleDbType.Boolean);
        parameter.Value.Should().Be(input);
    }

    [Fact]
    public void Parse_SupportsBoolNumericAndOracleBoolean()
    {
        var sut = new BooleanTypeHandler();

        sut.Parse(true).Should().BeTrue();
        sut.Parse(false).Should().BeFalse();
        sut.Parse(1).Should().BeTrue();
        sut.Parse(0).Should().BeFalse();
        sut.Parse(OracleBoolean.True).Should().BeTrue();
        sut.Parse(OracleBoolean.False).Should().BeFalse();
    }

    [Fact]
    public void AddTypeHandler_InfersGenericTypeFromTypeHandlerInstance()
    {
        var handler = new BooleanTypeHandler();

        OracleTypeMapper.AddTypeHandler(handler);

        OracleTypeMapper.HasTypeHandler(typeof(bool), out var registered).Should().BeTrue();
        registered.Should().BeSameAs(handler);
    }
}
