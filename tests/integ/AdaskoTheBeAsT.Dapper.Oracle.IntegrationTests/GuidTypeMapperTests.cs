using System;
using AdaskoTheBeAsT.Dapper.Oracle.IntegrationTests.Util;
using AdaskoTheBeAsT.Dapper.Oracle.TypeHandler;
using AwesomeAssertions;
using Dapper;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.Oracle.IntegrationTests;

[Collection("OracleDocker")]
public class GuidTypeMapperTests
{
    private readonly Guid _customerId = Guid.NewGuid();

    public GuidTypeMapperTests(DatabaseFixture fixture)
    {
        ArgumentNullException.ThrowIfNull(fixture);

        Fixture = fixture;
        OracleTypeMapper.AddTypeHandler<Guid>(new GuidRaw16TypeHandler());

        var columns = new[]
        {
            new TableColumn
            {
                Name = "CustomerId",
                DataType = OracleMappingType.Raw,
                Size = 16,
                PrimaryKey = true,
            },
            new TableColumn
            {
                Name = "Name",
                DataType = OracleMappingType.Varchar2,
                Size = 40,
            },
            new TableColumn
            {
                Name = "City",
                DataType = OracleMappingType.Varchar2,
                Size = 40,
            },
            new TableColumn
            {
                Name = "OtherGuid",
                DataType = OracleMappingType.Raw,
                Size = 16,
                Nullable = true,
            },
        };

        TableCreator.Create(Fixture.Connection, "GuidCustomerTest", columns);
        InsertCustomer(
            new GuidCustomer
            {
                CustomerId = _customerId,
                Name = "DIPS AS",
                City = "Oslo",
            });
    }

    private DatabaseFixture Fixture { get; }

    [Fact]
    [Trait("Category", "Integration")]
    public void InsertGuidTestTest()
    {
        var rowCount = InsertCustomer(
            new GuidCustomer
            {
                CustomerId = Guid.NewGuid(),
                Name = "DIPS AS",
                City = "Narvik",
            });
        rowCount.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void SelectGuidTest()
    {
        var selectParam = new OracleDynamicParameters
        {
            {
                "CustomerId", _customerId
            },
        };

        var customer = Fixture.Connection.QuerySingle<GuidCustomer>(
            "SELECT CustomerId,Name,City,OtherGuid FROM GuidCustomerTest WHERE CustomerId=:CustomerId",
            selectParam);
        customer.CustomerId.Should().Be(_customerId);
        customer.Name.Should().Be("DIPS AS");
        customer.City.Should().Be("Oslo");
        customer.OtherGuid.Should().BeEmpty();
    }

    private int InsertCustomer(GuidCustomer customer)
    {
        const string sql = "INSERT INTO GuidCustomerTest(CustomerId,Name,City) VALUES(:CustomerId,:Name,:City)";
        var param = new OracleDynamicParameters
        {
            {
                "CustomerId", customer.CustomerId
            },
            {
                "Name", customer.Name
            },
            {
                "City", customer.City
            },
        };

        return Fixture.Connection.Execute(sql, param);
    }
}
