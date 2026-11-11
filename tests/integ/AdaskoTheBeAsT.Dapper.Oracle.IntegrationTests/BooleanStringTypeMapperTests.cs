using System;
using AdaskoTheBeAsT.Dapper.Oracle.IntegrationTests.Util;
using AdaskoTheBeAsT.Dapper.Oracle.TypeHandler;
using AwesomeAssertions;
using Dapper;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.Oracle.IntegrationTests;

[Collection("OracleDocker")]
public class BooleanStringTypeMapperTests
{
    public BooleanStringTypeMapperTests(DatabaseFixture fixture)
    {
        ArgumentNullException.ThrowIfNull(fixture);

        Fixture = fixture;

        var columns = new[]
        {
            new NumberColumn
            {
                Name = "Id",
                DataType = OracleMappingType.Int32,
                PrimaryKey = true,
            },
            new TableColumn
            {
                Name = "BooleanValue",
                DataType = OracleMappingType.Char,
                Size = 1,
            },
        };
        OracleTypeMapper.AddTypeHandler<bool>(new BooleanStringTypeHandler("Y", "N"));
        TableCreator.Create(Fixture.Connection, "BoolCharTypeMappingTest", columns);
    }

    private DatabaseFixture Fixture { get; }

    [Fact]
    [Trait("Category", "Integration")]
    public void TestInsertMapping()
    {
        var count = InsertValue(1, booleanValue: true);
        count.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void TestSelectMappingFalse()
    {
        InsertValue(10, booleanValue: false);
        const string sql = "SELECT * FROM BoolCharTypeMappingTest WHERE ID=10";
        var actual = Fixture.Connection.QuerySingle<BoolTestMapping>(sql);
        actual.BooleanValue.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void TestSelectMappingTrue()
    {
        InsertValue(11, booleanValue: true);
        const string sql = "SELECT * FROM BoolCharTypeMappingTest WHERE ID=11";
        var actual = Fixture.Connection.QuerySingle<BoolTestMapping>(sql);
        actual.BooleanValue.Should().BeTrue();
    }

    private int InsertValue(
        int id,
        bool booleanValue)
    {
        const string sql = "INSERT INTO BoolCharTypeMappingTest(Id,BooleanValue) VALUES(:ID,:BOOL)";

        var parameters = new OracleDynamicParameters
        {
            {
                "ID", id
            },
            {
                "BOOL", booleanValue
            },
        };
        return Fixture.Connection.Execute(sql, parameters);
    }
}
