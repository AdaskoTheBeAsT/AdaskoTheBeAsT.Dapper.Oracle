using System;
using AdaskoTheBeAsT.Dapper.Oracle.TypeHandler;
using AwesomeAssertions;
using Oracle.ManagedDataAccess.Client;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.Oracle.Tests.TypeHandlerTests;

public class GuidRaw16TypeHandlerTests
{
    [Fact]
    public void ConvertTo()
    {
        var input = Guid.NewGuid();

        using var parameter = new OracleParameter();
        var sut = new GuidRaw16TypeHandler();
        sut.SetValue(parameter, input);

        parameter.Value.Should().BeEquivalentTo(input.ToByteArray());
        parameter.OracleDbType.Should().Be(OracleDbType.Raw);
    }

    [Fact]
    public void ConvertFrom()
    {
        var input = Guid.NewGuid();

        var sut = new GuidRaw16TypeHandler();
        var result = sut.Parse(input.ToByteArray());
        result.Should().Be(input);
    }

    [Fact]
    public void BigEndianMode_WritesAndParsesRfc4122NetworkByteOrder()
    {
        var input = Guid.Parse("00112233-4455-6677-8899-aabbccddeeff");
        byte[] expectedBigEndian =
        [
            0x00, 0x11, 0x22, 0x33,
            0x44, 0x55,
            0x66, 0x77,
            0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF,
        ];

        using var parameter = new OracleParameter();
        var sut = new GuidRaw16TypeHandler(useBigEndian: true);

        sut.UseBigEndian.Should().BeTrue();
        sut.SetValue(parameter, input);

        parameter.OracleDbType.Should().Be(OracleDbType.Raw);
        parameter.Size.Should().Be(16);
        parameter.Value.Should().BeEquivalentTo(expectedBigEndian, static options => options.WithStrictOrdering());

        sut.Parse(expectedBigEndian).Should().Be(input);
    }

    [Fact]
    public void Parse_SupportsHexAndUuidStringsAndThrowsOnUnsupportedType()
    {
        var input = Guid.NewGuid();
        var sut = new GuidRaw16TypeHandler();

        sut.Parse(input.ToString("N")).Should().Be(input);
        sut.Parse(input.ToString("D")).Should().Be(input);

        var act = () => sut.Parse(42);
        act.Should().Throw<NotSupportedException>();
    }
}
