using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Dapper.Oracle.BulkSql;
using AwesomeAssertions;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.Oracle.Tests;

public class BulkSqlTests
{
    [Fact]
    public void GuessType_MapsSupportedClrTypes()
    {
        OracleMapper.GuessType(typeof(Guid)).Should().Be(OracleMappingType.Raw);
        OracleMapper.GuessType(typeof(Guid?)).Should().Be(OracleMappingType.Raw);
        OracleMapper.GuessType(typeof(byte[])).Should().Be(OracleMappingType.Raw);
        OracleMapper.GuessType(typeof(string)).Should().Be(OracleMappingType.Varchar2);
        OracleMapper.GuessType(typeof(short)).Should().Be(OracleMappingType.Int16);
        OracleMapper.GuessType(typeof(int)).Should().Be(OracleMappingType.Int32);
        OracleMapper.GuessType(typeof(long)).Should().Be(OracleMappingType.Int64);
        OracleMapper.GuessType(typeof(float)).Should().Be(OracleMappingType.BinaryFloat);
        OracleMapper.GuessType(typeof(double)).Should().Be(OracleMappingType.BinaryDouble);
        OracleMapper.GuessType(typeof(decimal)).Should().Be(OracleMappingType.Decimal);
        OracleMapper.GuessType(typeof(DateTime)).Should().Be(OracleMappingType.Date);
        OracleMapper.GuessType(typeof(DateTimeOffset)).Should().Be(OracleMappingType.TimeStampTZ);
        OracleMapper.GuessType(typeof(TimeSpan)).Should().Be(OracleMappingType.IntervalDS);
        OracleMapper.GuessType(typeof(DateOnly)).Should().Be(OracleMappingType.Date);
        OracleMapper.GuessType(typeof(DateOnly?)).Should().Be(OracleMappingType.Date);
        OracleMapper.GuessType(typeof(TimeOnly)).Should().Be(OracleMappingType.IntervalDS);
        OracleMapper.GuessType(typeof(TimeOnly?)).Should().Be(OracleMappingType.IntervalDS);
        OracleMapper.GuessType(typeof(bool)).Should().Be(OracleMappingType.Int16);
        OracleMapper.GuessType(typeof(float[])).Should().Be(OracleMappingType.VectorFloat32);
        OracleMapper.GuessType(typeof(double[])).Should().Be(OracleMappingType.VectorFloat64);
        OracleMapper.GuessType(typeof(object)).Should().BeNull();
    }

    [Fact]
    public void CreateParameterFromObject_BindsInferredTypesOnOracleCommand()
    {
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();
        var items = new List<BulkSampleRow>
        {
            new()
            {
                Id = firstId,
                Name = "Alice",
                Timestamp = 100L,
            },
            new()
            {
                Id = secondId,
                Name = "Bob",
                Timestamp = 200L,
            },
        };

        var mapping = new BulkMapping<BulkSampleRow>[]
        {
            new(":Id", static x => x.Id),
            new("@Name", static x => x.Name),
            new("Timestamp", static x => x.Timestamp),
        };

        var dynamicParams = BulkOperation.CreateParameterFromObject(items, mapping);
        dynamicParams.ArrayBindCount.Should().Be(2);

        using var command = new OracleCommand();
        ((SqlMapper.IDynamicParameters)dynamicParams).AddParameters(command, null!);

        command.ArrayBindCount.Should().Be(2);
        command.Parameters.Count.Should().Be(3);

        command.Parameters["Id"].OracleDbType.Should().Be(OracleDbType.Raw);
        command.Parameters["Id"]
            .Value.Should()
            .BeEquivalentTo(
                new object[]
                {
                    firstId.ToByteArray(),
                    secondId.ToByteArray(),
                });

        command.Parameters["Name"].OracleDbType.Should().Be(OracleDbType.Varchar2);
        command.Parameters["Name"]
            .Value.Should()
            .BeEquivalentTo(
                new object[]
                {
                    "Alice",
                    "Bob",
                });

        command.Parameters["Timestamp"].OracleDbType.Should().Be(OracleDbType.Int64);
        command.Parameters["Timestamp"]
            .Value.Should()
            .BeEquivalentTo(
                new object[]
                {
                    100L,
                    200L,
                });
    }

    [Fact]
    public void OracleMapperCreate_UsesReflectedProperties()
    {
        var id = Guid.NewGuid();
        var items = new List<BulkSampleRow>
        {
            new()
            {
                Id = id,
                Name = "Reflected",
                Timestamp = 42L,
                IsActive = true,
            },
        };

        var dynamicParams = OracleMapper.Create(items);

        using var command = new OracleCommand();
        ((SqlMapper.IDynamicParameters)dynamicParams).AddParameters(command, null!);

        command.ArrayBindCount.Should().Be(1);
        command.Parameters["Id"].OracleDbType.Should().Be(OracleDbType.Raw);
        command.Parameters["Id"]
            .Value.Should()
            .BeEquivalentTo(
                new object[]
                {
                    id.ToByteArray(),
                });
        command.Parameters["Name"].OracleDbType.Should().Be(OracleDbType.Varchar2);
        command.Parameters["Timestamp"].OracleDbType.Should().Be(OracleDbType.Int64);
        command.Parameters["IsActive"].OracleDbType.Should().Be(OracleDbType.Int16);
    }

    [Fact]
    public void OracleMapperCreate_NormalizesDateOnlyTimeOnlyAndNullableGuid()
    {
        var nullableId = Guid.NewGuid();
        var items = new List<ModernBulkSampleRow>
        {
            new()
            {
                OptionalId = nullableId,
                BirthDate = new DateOnly(2026, 4, 15),
                StartTime = new TimeOnly(9, 15, 0),
                Embedding = [0.1f, 0.2f, 0.3f],
            },
            new()
            {
                OptionalId = null,
                BirthDate = null,
                StartTime = null,
                Embedding = [0.4f, 0.5f, 0.6f],
            },
        };

        var dynamicParams = OracleMapper.Create(items);

        using var command = new OracleCommand();
        ((SqlMapper.IDynamicParameters)dynamicParams).AddParameters(command, null!);

        command.ArrayBindCount.Should().Be(2);
        command.Parameters["OptionalId"].OracleDbType.Should().Be(OracleDbType.Raw);
        command.Parameters["OptionalId"]
            .Value.Should()
            .BeEquivalentTo(
                new object?[]
                {
                    nullableId.ToByteArray(),
                    null,
                });
        command.Parameters["BirthDate"].OracleDbType.Should().Be(OracleDbType.Date);
        command.Parameters["BirthDate"]
            .Value.Should()
            .BeEquivalentTo(
                new object?[]
                {
                    new DateTime(2026, 4, 15, 0, 0, 0, DateTimeKind.Unspecified),
                    null,
                });
        command.Parameters["StartTime"].OracleDbType.Should().Be(OracleDbType.IntervalDS);
        command.Parameters["StartTime"]
            .Value.Should()
            .BeEquivalentTo(
                new object?[]
                {
                    new TimeSpan(9, 15, 0),
                    null,
                });
        command.Parameters["Embedding"].OracleDbType.Should().Be(OracleDbType.Vector_Float32);
    }

    [Fact]
    public void SqlBulk_EmptyObjects_DoesNotExecuteCommand()
    {
        using var connection = new OracleConnection();
        var mapping = new BulkMapping<BulkSampleRow>[]
        {
            new("Id", static x => x.Id),
        };

        var syncResult = connection.SqlBulk(
            "INSERT INTO T(Id) VALUES (:Id)",
            Array.Empty<BulkSampleRow>(),
            mapping);
        syncResult.Should().Be(0);
    }

    [Fact]
    public async Task SqlBulkAsync_EmptyObjects_DoesNotExecuteCommandAsync()
    {
        await using var connection = new OracleConnection();
        var mapping = new BulkMapping<BulkSampleRow>[]
        {
            new("Id", static x => x.Id),
        };

        var asyncResult = await connection.SqlBulkAsync(
            "INSERT INTO T(Id) VALUES (:Id)",
            Array.Empty<BulkSampleRow>(),
            mapping);
        asyncResult.ExecuteResult.Should().Be(0);
        asyncResult.Parameters.Should().NotBeNull();
        asyncResult.Parameters.ArrayBindCount.Should().Be(0);
    }

    private sealed class BulkSampleRow
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }

        public long Timestamp { get; set; }

        public bool IsActive { get; set; }
    }

    private sealed class ModernBulkSampleRow
    {
        public Guid? OptionalId { get; set; }

        public DateOnly? BirthDate { get; set; }

        public TimeOnly? StartTime { get; set; }

        public float[]? Embedding { get; set; }
    }
}
