using System;
using System.Text;
using AwesomeAssertions;
using Oracle.ManagedDataAccess.Types;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.Oracle.Tests;

public class OracleValueConverterTests
{
    [Fact]
    public void GetOracleBoolean()
    {
        const bool isTrue = true;
        var oraBool = new OracleBoolean(isTrue);
        var result = OracleValueConverter.Convert<bool>(oraBool);
        result.Should().BeTrue();

        oraBool = new OracleBoolean(-1);
        result = OracleValueConverter.Convert<bool>(oraBool);
        result.Should().BeTrue();

        oraBool = new OracleBoolean(1);
        result = OracleValueConverter.Convert<bool>(oraBool);
        result.Should().BeTrue();

        oraBool = new OracleBoolean(0);
        result = OracleValueConverter.Convert<bool>(oraBool);
        result.Should().BeFalse();

        var oraBoolArr = new OracleBoolean[]
        {
            true,
            false,
        };
        var resultArr = OracleValueConverter.Convert<bool[]>(oraBoolArr);
        resultArr.Should().BeOfType<bool[]>();
        resultArr.Should().HaveCount(2);
    }

    [Fact]
    public void GetOracleBinary()
    {
        var bytes = Encoding.UTF8.GetBytes("Lorem ipsum");
        var oracleBytes = new OracleBinary(bytes);
        var result = OracleValueConverter.Convert<byte[]>(oracleBytes);
        result.Should().Equal(bytes);
    }

    [Fact]
    public void GetOracleTimeStamp()
    {
        var dateNow = DateTime.Now;
        var oraTimeStamp = new OracleTimeStamp(dateNow);
        var result = OracleValueConverter.Convert<DateTime>(oraTimeStamp);
        result.Should().Be(dateNow);

        var oraTimeStampArr = new OracleTimeStamp[]
        {
            dateNow,
        };
        var secondResult = OracleValueConverter.Convert<DateTime[]>(oraTimeStampArr);
        secondResult.Should().BeOfType<DateTime[]>();
        secondResult.Should().ContainSingle();
    }

    [Fact]
    public void GetOracleDateArray()
    {
        var oraDateArray = new OracleDate[]
        {
            (OracleDate)DateTime.Now,
            (OracleDate)DateTime.Now.AddYears(1),
            new OracleDate("10/29/2020 16:14:23"),
        };

        var nullableOraDateArray = new OracleDate?[]
        {
            (OracleDate)DateTime.Now,
            (OracleDate)DateTime.Now.AddYears(1),
            null,
            new OracleDate("10/29/2020 16:14:23"),
        };

        var result = OracleValueConverter.Convert<DateTime[]>(oraDateArray);
        result.Should().BeOfType<DateTime[]>();
        result.Should().HaveCount(oraDateArray.Length);

        var secondResult = OracleValueConverter.Convert<DateTime?[]>(nullableOraDateArray);
        secondResult.Should().BeOfType<DateTime?[]>();
        secondResult.Should().HaveCount(nullableOraDateArray.Length);
    }

    [Fact]
    public void GetStringArray()
    {
        var oraArray = new[]
        {
            "Foo",
            "Bar",
        };
        var result = OracleValueConverter.Convert<string[]>(oraArray);
        result.Should().BeOfType<string[]>();
        result.Should().HaveCount(2);

        var nullableArray = new string?[]
        {
            "Foo",
            "Bar",
            null,
        };
        var nullableResult = OracleValueConverter.Convert<string?[]>(nullableArray);
        nullableResult.Should().BeOfType<string?[]>();
        nullableResult.Should().HaveCount(3);
    }

    [Fact]
    public void GetOracleStringArray()
    {
        var oraArray = new OracleString[]
        {
            "Foo",
            "Bar",
        };
        var result = OracleValueConverter.Convert<OracleString[]>(oraArray);
        result.Should().BeOfType<OracleString[]>();
        result.Should().HaveCount(2);
    }

    [Fact]
    public void GetOracleString2StringArray()
    {
        var oraArray = new OracleString[]
        {
            "Foo",
            "Bar",
        };
        var result = OracleValueConverter.Convert<string[]>(oraArray);
        result.Should().BeOfType<string[]>();
        result.Should().HaveCount(2);
    }

    [Fact]
    public void GetIntArray()
    {
        var oraArray = new OracleDecimal[]
        {
            1,
            2,
        };
        var result = OracleValueConverter.Convert<int[]>(oraArray);
        result.Should().BeOfType<int[]>();
        result.Should().HaveCount(2);
    }

    [Fact]
    public void GetOracleStringWithContentReturnsContent()
    {
        var oraString = new OracleString("Foo");
        var result = OracleValueConverter.Convert<string>(oraString);
        result.Should().Be("Foo");
    }

    [Fact]
    public void GetOracleStringAsNullReturnsNull()
    {
        var oraString = default(OracleString);
        var result = OracleValueConverter.Convert<string>(oraString);
        result.Should().BeNull();
    }

    [Fact]
    public void GetStringAsDbNullReturnsNull()
    {
        var result = OracleValueConverter.Convert<string>(DBNull.Value);
        result.Should().BeNull();
    }

    [Fact]
    public void GetNullAsStringReturnsNull()
    {
        var result = OracleValueConverter.Convert<string>(value: null);
        result.Should().BeNull();
    }

    [Fact]
    public void GetNullAsDecimalReturns0()
    {
        var result = OracleValueConverter.Convert<decimal>(value: null);
        result.Should().Be(0);
    }

    [Fact]
    public void GetDbNullAsDecimalReturns0()
    {
        var result = OracleValueConverter.Convert<decimal>(DBNull.Value);
        result.Should().Be(0);
    }

    [Fact]
    public void GetNullAsInt32Returns0()
    {
        var result = OracleValueConverter.Convert<int>(value: null);
        result.Should().Be(0);
    }

    [Fact]
    public void GetDbNullAsInt32Returns0()
    {
        var result = OracleValueConverter.Convert<int>(DBNull.Value);
        result.Should().Be(0);
    }

    [Fact]
    public void GetNullAsInt64Returns0()
    {
        var result = OracleValueConverter.Convert<long>(value: null);
        result.Should().Be(0);
    }

    [Fact]
    public void GetDbNullAsInt64Returns0()
    {
        var result = OracleValueConverter.Convert<long>(DBNull.Value);
        result.Should().Be(0);
    }

    [Fact]
    public void GetNullAsNullableDecimalReturnsNull()
    {
        var result = OracleValueConverter.Convert<decimal?>(value: null);
        result.Should().BeNull();
    }

    [Fact]
    public void GetNullAsNullableInt32ReturnsNull()
    {
        var result = OracleValueConverter.Convert<int?>(value: null);
        result.Should().BeNull();
    }

    [Fact]
    public void GetNullAsNullableInt64ReturnsNull()
    {
        var result = OracleValueConverter.Convert<long?>(value: null);
        result.Should().BeNull();
    }

    [Fact]
    public void GetDbNullAsDecimalReturnsNull()
    {
        var result = OracleValueConverter.Convert<decimal?>(DBNull.Value);
        result.Should().BeNull();
    }

    [Fact]
    public void GetDbNullAsInt32ReturnsNull()
    {
        var result = OracleValueConverter.Convert<int?>(DBNull.Value);
        result.Should().BeNull();
    }

    [Fact]
    public void GetDbNullAsInt64ReturnsNull()
    {
        var result = OracleValueConverter.Convert<long?>(DBNull.Value);
        result.Should().BeNull();
    }

    [Fact]
    public void GetOracleDecimalReturnsDecimal()
    {
        const decimal expected = 100;

        var result = OracleValueConverter.Convert<decimal>(new OracleDecimal(expected));

        result.Should().Be(expected);
    }

    [Fact]
    public void GetOracleDecimalReturnsNullableDecimal()
    {
        const decimal expected = 100;

        var result = OracleValueConverter.Convert<decimal?>(new OracleDecimal(expected));

        result.Should().Be(expected);
    }

    [Fact]
    public void GetOracleDecimalReturnsInt32()
    {
        const decimal input = 100;
        var expected = Convert.ToInt32(input);

        var result = OracleValueConverter.Convert<int>(new OracleDecimal(input));

        result.Should().Be(expected);
    }

    [Fact]
    public void GetOracleDecimalReturnsNullableInt32()
    {
        const decimal input = 100;
        var expected = Convert.ToInt32(input);

        var result = OracleValueConverter.Convert<int?>(new OracleDecimal(input));

        result.Should().Be(expected);
    }

    [Fact]
    public void GetOracleDecimalReturnsInt64()
    {
        const decimal input = 100;
        var expected = Convert.ToInt64(input);

        var result = OracleValueConverter.Convert<long>(new OracleDecimal(input));

        result.Should().Be(expected);
    }

    [Fact]
    public void GetOracleDecimalReturnsNullableInt64()
    {
        const decimal input = 100;
        var expected = Convert.ToInt64(input);

        var result = OracleValueConverter.Convert<long?>(new OracleDecimal(input));

        result.Should().Be(expected);
    }

    [Fact]
    public void GetOracleDateTimeReturnsDateTime()
    {
        var result = OracleValueConverter.Convert<DateTime>(new OracleDate(DateTime.Today));
        result.Should().Be(DateTime.Today);
    }

    [Fact]
    public void GetOracleIntervalDsReturnsTimeSpan()
    {
        var expected = new TimeSpan(2, 3, 4, 5, 250);
        var oracleInterval = new OracleIntervalDS(expected);

        OracleValueConverter.Convert<TimeSpan>(oracleInterval).Should().Be(expected);
        OracleValueConverter.Convert<TimeSpan?>(oracleInterval).Should().Be(expected);
        OracleValueConverter.Convert<TimeSpan?>(OracleIntervalDS.Null).Should().BeNull();
    }

    [Fact]
    public void GetOracleTimeStampTzReturnsDateTimeOffsetWithExactOffset()
    {
        var oracleTsTz = new OracleTimeStampTZ(2026, 4, 15, 14, 30, 45, 0, "+02:00");

        var result = OracleValueConverter.Convert<DateTimeOffset>(oracleTsTz);
        var nullableResult = OracleValueConverter.Convert<DateTimeOffset?>(oracleTsTz);

        result.Should().Be(new DateTimeOffset(2026, 4, 15, 14, 30, 45, TimeSpan.FromHours(2)));
        nullableResult.Should().Be(result);
        OracleValueConverter.Convert<DateTimeOffset?>(OracleTimeStampTZ.Null).Should().BeNull();
    }

    [Fact]
    public void GetOracleVectorReturnsFloatArrayOrOracleVector()
    {
        float[] embedding = [0.25f, -0.5f, 0.75f];
        using var vector = new OracleVector(embedding);

        OracleValueConverter.Convert<float[]>(vector).Should().Equal(embedding);
        OracleValueConverter.Convert<OracleVector>(vector).ToFloatArray().Should().Equal(embedding);
        OracleValueConverter.Convert<float[]>(OracleVector.Null).Should().BeNull();
    }

    [Fact]
    public void GetOracleBinary16BytesReturnsGuid()
    {
        var expected = Guid.NewGuid();
        var binary = new OracleBinary(expected.ToByteArray());

        OracleValueConverter.Convert<Guid>(binary).Should().Be(expected);
        OracleValueConverter.Convert<Guid?>(binary).Should().Be(expected);
        OracleValueConverter.Convert<Guid?>(OracleBinary.Null).Should().BeNull();
    }

    [Fact]
    public void GetOracleTypesReturnDateOnlyAndTimeOnly()
    {
        var expectedDate = new DateOnly(2026, 4, 15);
        var expectedTime = new TimeOnly(14, 30, 45, 125);

        OracleValueConverter.Convert<DateOnly>(new OracleDate(2026, 4, 15)).Should().Be(expectedDate);
        OracleValueConverter.Convert<DateOnly?>(OracleDate.Null).Should().BeNull();

        OracleValueConverter.Convert<TimeOnly>(new OracleIntervalDS(expectedTime.ToTimeSpan()))
            .Should()
            .Be(expectedTime);
        OracleValueConverter.Convert<TimeOnly?>(OracleIntervalDS.Null).Should().BeNull();
    }
}
