using System;
using System.Data;
using AdaskoTheBeAsT.Dapper.Oracle.TypeHandler;
using AwesomeAssertions;
using Dapper;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.Oracle.IntegrationTests;

[Collection("OracleDocker")]
public class Oracle23aiFeaturesTests
{
    public Oracle23aiFeaturesTests(DatabaseFixture fixture)
    {
        ArgumentNullException.ThrowIfNull(fixture);

        Fixture = fixture;
        OracleTypeMapper.AddTypeHandler<DateOnly>(new DateOnlyTypeHandler());
        OracleTypeMapper.AddTypeHandler<TimeOnly>(new TimeOnlyTypeHandler());

        const string dropSql = """
                               BEGIN
                                   EXECUTE IMMEDIATE 'DROP TABLE Oracle23aiFeatureTest';
                               EXCEPTION
                                   WHEN OTHERS THEN
                                       IF SQLCODE != -942 THEN
                                           RAISE;
                                       END IF;
                               END;
                               """;
        Fixture.Connection.Execute(dropSql.Replace("\r\n", "\n", StringComparison.Ordinal));

        const string createSql = """
                                 CREATE TABLE Oracle23aiFeatureTest (
                                     Id NUMBER(10) PRIMARY KEY,
                                     IsActive BOOLEAN NOT NULL,
                                     EventDate DATE NOT NULL,
                                     EventTime INTERVAL DAY TO SECOND NOT NULL,
                                     Payload JSON NOT NULL,
                                     LargeNotes CLOB NOT NULL,
                                     Embedding VECTOR(3, FLOAT32) NOT NULL
                                 )
                                 """;
        Fixture.Connection.Execute(createSql.Replace("\r\n", "\n", StringComparison.Ordinal));
    }

    private DatabaseFixture Fixture { get; }

    [Fact]
    [Trait("Category", "Integration")]
    public void InsertAndQuery_Oracle23aiNativeTypesAndInlineLobFetch()
    {
        var expectedDate = new DateOnly(2026, 4, 15);
        var expectedTime = new TimeOnly(14, 30, 45);
        var expectedNotes = new string('N', 12_000);
        float[] expectedVector = [0.25f, 0.5f, 0.75f];

        var insertParams = new OracleDynamicParameters
        {
            BindByName = true,
        };
        insertParams.Add("Id", 1, OracleMappingType.Int32);
        insertParams.Add("IsActive", value: true, OracleMappingType.Boolean);
        insertParams.Add("EventDate", expectedDate.ToDateTime(TimeOnly.MinValue), OracleMappingType.Date);
        insertParams.Add("EventTime", expectedTime.ToTimeSpan(), OracleMappingType.IntervalDS);
        insertParams.Add("Payload", "{\"tier\":\"gold\"}", OracleMappingType.Json);
        insertParams.Add("LargeNotes", expectedNotes, OracleMappingType.Clob);
        insertParams.Add("Embedding", expectedVector, OracleMappingType.VectorFloat32);

        const string insertSql = """
                                 INSERT INTO Oracle23aiFeatureTest (
                                     Id, IsActive, EventDate, EventTime, Payload, LargeNotes, Embedding
                                 ) VALUES (
                                     :Id, :IsActive, :EventDate, :EventTime, :Payload, :LargeNotes, :Embedding
                                 )
                                 """;

        var inserted = Fixture.Connection.Execute(
            insertSql.Replace("\r\n", "\n", StringComparison.Ordinal),
            insertParams);
        inserted.Should().Be(1);

        var selectParams = new OracleDynamicParameters
        {
            BindByName = true,
            InitialLOBFetchSize = -1,
            FetchSize = 131_072L,
        };
        selectParams.Add("Id", 1, OracleMappingType.Int32, ParameterDirection.Input);

        const string selectSql = """
                                 SELECT
                                     Id,
                                     EventDate,
                                     EventTime,
                                     JSON_SERIALIZE(Payload RETURNING VARCHAR2(4000)) AS PayloadJson,
                                     LargeNotes
                                 FROM Oracle23aiFeatureTest
                                 WHERE Id = :Id
                                 """;

        var row = Fixture.Connection.QuerySingle<Oracle23aiProjection>(
            selectSql.Replace("\r\n", "\n", StringComparison.Ordinal),
            selectParams);

        row.Id.Should().Be(1);
        row.EventDate.Should().Be(expectedDate);
        row.EventTime.Should().Be(expectedTime);
        row.PayloadJson.Should().Contain("\"tier\":\"gold\"");
        row.LargeNotes.Should().Be(expectedNotes);
    }
}
