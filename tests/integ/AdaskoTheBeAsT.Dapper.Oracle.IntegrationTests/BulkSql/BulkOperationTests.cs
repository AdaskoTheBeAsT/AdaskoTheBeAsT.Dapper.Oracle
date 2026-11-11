using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Dapper.Oracle.BulkSql;
using AdaskoTheBeAsT.Dapper.Oracle.IntegrationTests.Util;
using AwesomeAssertions;
using Dapper;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.Oracle.IntegrationTests.BulkSql;

[Collection("OracleDocker")]
public class BulkOperationTests
{
    private const string InsertSql =
        "Insert into BULKCUSTOMERS (CUSTOMERID,COMPANYNAME,CITY,CONTACTNAME,CONTACTTITLE,ADDRESS,POSTALCODE,COUNTRY,PHONE,FAX,TIDSSTEMPEL,OPPRETTETAV,OPPRETTETTID,SISTENDRETAV,SISTENDRETTID,DIPSID) "
        + "values (:CUSTOMERID,:COMPANYNAME,:CITY,:CONTACTNAME,:CONTACTTITLE,:ADDRESS,:POSTALCODE,:COUNTRY,:PHONE,:FAX,:TIDSSTEMPEL,:OPPRETTETAV,:OPPRETTETTID,:SISTENDRETAV,:SISTENDRETTID,:DIPSID)";

    public BulkOperationTests(DatabaseFixture fixture)
    {
        ArgumentNullException.ThrowIfNull(fixture);

        Fixture = fixture;

        var columns = new[]
        {
            new TableColumn
            {
                Name = "CUSTOMERID",
                DataType = OracleMappingType.Raw,
                Size = 16,
                PrimaryKey = true,
            },
            new TableColumn
            {
                Name = "COMPANYNAME",
                DataType = OracleMappingType.Varchar2,
                Size = 40,
            },
            new TableColumn
            {
                Name = "CITY",
                DataType = OracleMappingType.Varchar2,
                Size = 40,
            },
            new TableColumn
            {
                Name = "CONTACTNAME",
                DataType = OracleMappingType.Varchar2,
                Size = 40,
            },
            new TableColumn
            {
                Name = "CONTACTTITLE",
                DataType = OracleMappingType.Varchar2,
                Size = 40,
            },
            new TableColumn
            {
                Name = "ADDRESS",
                DataType = OracleMappingType.Varchar2,
                Size = 60,
            },
            new TableColumn
            {
                Name = "POSTALCODE",
                DataType = OracleMappingType.Varchar2,
                Size = 40,
                Nullable = true,
            },
            new TableColumn
            {
                Name = "COUNTRY",
                DataType = OracleMappingType.Varchar2,
                Size = 40,
            },
            new TableColumn
            {
                Name = "PHONE",
                DataType = OracleMappingType.Varchar2,
                Size = 40,
                Nullable = true,
            },
            new TableColumn
            {
                Name = "FAX",
                DataType = OracleMappingType.Varchar2,
                Size = 40,
                Nullable = true,
            },
            new NumberColumn
            {
                Name = "TIDSSTEMPEL",
                DataType = OracleMappingType.Int64,
                Nullable = true,
            },
            new TableColumn
            {
                Name = "OPPRETTETAV",
                DataType = OracleMappingType.Varchar2,
                Size = 40,
                Nullable = true,
            },
            new TableColumn
            {
                Name = "OPPRETTETTID",
                DataType = OracleMappingType.Date,
                Nullable = true,
            },
            new TableColumn
            {
                Name = "SISTENDRETAV",
                DataType = OracleMappingType.Varchar2,
                Size = 40,
                Nullable = true,
            },
            new TableColumn
            {
                Name = "SISTENDRETTID",
                DataType = OracleMappingType.Date,
                Nullable = true,
            },
            new NumberColumn
            {
                Name = "DIPSID",
                DataType = OracleMappingType.Int64,
                Nullable = true,
            },
        };

        TableCreator.Create(Fixture.Connection, "BULKCUSTOMERS", columns);
    }

    private DatabaseFixture Fixture { get; }

    [Fact]
    [Trait("Category", "IntegrationTest")]
    public void BulkSql()
    {
        var customers = GetCustomersFromEmbeddedResource();
        foreach (var customer in customers)
        {
            customer.CustomerId = Guid.NewGuid();
        }

        var customerCount = customers.Count;

        var result = Fixture.Connection.SqlBulk(InsertSql, customers, CreateMapping());
        result.Should().Be(customerCount);
    }

    [Fact]
    [Trait("Category", "IntegrationTest")]
    public async Task BulkSqlAsync()
    {
        var customers = GetCustomersFromEmbeddedResource();
        foreach (var customer in customers)
        {
            customer.CustomerId = Guid.NewGuid();
        }

        var customerCount = customers.Count;

        var asyncQueryResult = await Fixture.Connection.SqlBulkAsync(InsertSql, customers, CreateMapping());
        asyncQueryResult.ExecuteResult.Should().Be(customerCount);
        asyncQueryResult.Parameters.Should().NotBeNull();

        var mexicoCity = await Fixture.Connection.QuerySingleAsync<string>(
            "SELECT DISTINCT CITY FROM BULKCUSTOMERS WHERE COMPANYNAME = 'Ana Trujillo Emparedados y helados'");
        mexicoCity.Should().Be("México D.F.");

        var arhusCity = await Fixture.Connection.QuerySingleAsync<string>(
            "SELECT DISTINCT CITY FROM BULKCUSTOMERS WHERE COMPANYNAME = 'Vaffeljernet'");
        arhusCity.Should().Be("Århus");
    }

    [Fact]
    [Trait("Category", "IntegrationTest")]
    public void BulkInsert_Test()
    {
        var customers = GetCustomersFromEmbeddedResource();
        foreach (var customer in customers)
        {
            customer.CustomerId = Guid.NewGuid();
        }

        var customerCount = customers.Count;

        var dal = new CustomerDal(connection: Fixture.Connection);
        var inserted = dal.InsertCustomers(customers);
        inserted.Should().Be(customerCount);
    }

    [Fact]
    [Trait("Category", "IntegrationTest")]
    public void BulkSql_EmptyCollection_ReturnsZero()
    {
        var emptyCustomers = Array.Empty<Customer>();

        var syncResult = Fixture.Connection.SqlBulk(InsertSql, emptyCustomers, CreateMapping());
        syncResult.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "IntegrationTest")]
    public async Task BulkSqlAsync_EmptyCollection_ReturnsZeroAsync()
    {
        var emptyCustomers = Array.Empty<Customer>();

        var asyncResult = await Fixture.Connection.SqlBulkAsync(InsertSql, emptyCustomers, CreateMapping());
        asyncResult.ExecuteResult.Should().Be(0);
        asyncResult.Parameters.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "IntegrationTest")]
    public async Task BulkSqlAsync_ModernClrAndOracle23aiTypes_InsertsAllRowsAsync()
    {
        const string dropSql = """
                               BEGIN
                                   EXECUTE IMMEDIATE 'DROP TABLE BULKMODERN23AI';
                               EXCEPTION
                                   WHEN OTHERS THEN
                                       IF SQLCODE != -942 THEN
                                           RAISE;
                                       END IF;
                               END;
                               """;
        await Fixture.Connection.ExecuteAsync(dropSql.Replace("\r\n", "\n", StringComparison.Ordinal));

        const string createSql = """
                                 CREATE TABLE BULKMODERN23AI (
                                     ID RAW(16) PRIMARY KEY,
                                     OPTIONALID RAW(16) NULL,
                                     SHORTVAL NUMBER(5) NOT NULL,
                                     FLOATVAL BINARY_FLOAT NOT NULL,
                                     DOUBLEVAL BINARY_DOUBLE NOT NULL,
                                     NUMERICFLAG NUMBER(1) NOT NULL,
                                     NATIVEFLAG BOOLEAN NOT NULL,
                                     CREATEDAT TIMESTAMP WITH TIME ZONE NOT NULL,
                                     DURATION INTERVAL DAY TO SECOND NOT NULL,
                                     EVENTDATE DATE NULL,
                                     EVENTTIME INTERVAL DAY TO SECOND NULL,
                                     EMBEDDING32 VECTOR(3, FLOAT32) NOT NULL,
                                     EMBEDDING64 VECTOR(2, FLOAT64) NOT NULL
                                 )
                                 """;
        await Fixture.Connection.ExecuteAsync(createSql.Replace("\r\n", "\n", StringComparison.Ordinal));

        var rows = new List<ModernBulkRow>
        {
            new()
            {
#if NET9_0_OR_GREATER
                Id = Guid.CreateVersion7(),
#else
                Id = Guid.NewGuid(),
#endif
                OptionalId = Guid.NewGuid(),
                ShortVal = 12,
                FloatVal = 1.5f,
                DoubleVal = 2.75d,
                NumericFlag = true,
                NativeFlag = true,
                CreatedAt = new DateTimeOffset(2026, 4, 18, 12, 0, 0, TimeSpan.FromHours(2)),
                Duration = TimeSpan.FromMinutes(45),
                EventDate = new DateOnly(2026, 4, 18),
                EventTime = new TimeOnly(9, 30, 0),
                Embedding32 = [0.1f, 0.2f, 0.3f],
                Embedding64 = [0.4d, 0.5d],
            },
            new()
            {
#if NET9_0_OR_GREATER
                Id = Guid.CreateVersion7(),
#else
                Id = Guid.NewGuid(),
#endif
                OptionalId = null,
                ShortVal = -7,
                FloatVal = -0.25f,
                DoubleVal = 42.125d,
                NumericFlag = false,
                NativeFlag = false,
                CreatedAt = new DateTimeOffset(2026, 4, 19, 8, 15, 0, TimeSpan.Zero),
                Duration = TimeSpan.FromHours(2),
                EventDate = null,
                EventTime = null,
                Embedding32 = [0.6f, 0.7f, 0.8f],
                Embedding64 = [0.9d, 1.0d],
            },
        };

        var mapping = new BulkMapping<ModernBulkRow>[]
        {
            new("ID", static x => x.Id),
            new("OPTIONALID", static x => x.OptionalId),
            new("SHORTVAL", static x => x.ShortVal),
            new("FLOATVAL", static x => x.FloatVal),
            new("DOUBLEVAL", static x => x.DoubleVal),
            new("NUMERICFLAG", static x => x.NumericFlag),
            new("NATIVEFLAG", static x => x.NativeFlag, OracleMappingType.Boolean),
            new("CREATEDAT", static x => x.CreatedAt),
            new("DURATION", static x => x.Duration),
            new("EVENTDATE", static x => x.EventDate),
            new("EVENTTIME", static x => x.EventTime),
            new("EMBEDDING32", static x => x.Embedding32),
            new("EMBEDDING64", static x => x.Embedding64),
        };

        const string insertModernSql = """
                                       INSERT INTO BULKMODERN23AI (
                                           ID, OPTIONALID, SHORTVAL, FLOATVAL, DOUBLEVAL,
                                           NUMERICFLAG, NATIVEFLAG, CREATEDAT, DURATION,
                                           EVENTDATE, EVENTTIME, EMBEDDING32, EMBEDDING64
                                       ) VALUES (
                                           :ID, :OPTIONALID, :SHORTVAL, :FLOATVAL, :DOUBLEVAL,
                                           :NUMERICFLAG, :NATIVEFLAG, :CREATEDAT, :DURATION,
                                           :EVENTDATE, :EVENTTIME, :EMBEDDING32, :EMBEDDING64
                                       )
                                       """;

        var result = await Fixture.Connection.SqlBulkAsync(
            insertModernSql.Replace("\r\n", "\n", StringComparison.Ordinal),
            rows,
            mapping);
        result.ExecuteResult.Should().Be(2);

        var count = await Fixture.Connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM BULKMODERN23AI");
        count.Should().Be(2);
    }

    private static IEnumerable<BulkMapping<Customer>> CreateMapping()
    {
        yield return new BulkMapping<Customer>(
            "CUSTOMERID",
            static c => c.CustomerId.ToByteArray(),
            OracleMappingType.Raw);
        yield return new BulkMapping<Customer>("COMPANYNAME", static c => c.CompanyName, OracleMappingType.Varchar2);
        yield return new BulkMapping<Customer>("CITY", static c => c.City, OracleMappingType.Varchar2);
        yield return new BulkMapping<Customer>("CONTACTNAME", static c => c.ContactName, OracleMappingType.Varchar2);
        yield return new BulkMapping<Customer>("CONTACTTITLE", static c => c.ContactTitle, OracleMappingType.Varchar2);
        yield return new BulkMapping<Customer>("ADDRESS", static c => c.Address, OracleMappingType.Varchar2);
        yield return new BulkMapping<Customer>("POSTALCODE", static c => c.PostalCode, OracleMappingType.Varchar2);
        yield return new BulkMapping<Customer>("COUNTRY", static c => c.Country, OracleMappingType.Varchar2);
        yield return new BulkMapping<Customer>("PHONE", static c => c.Phone, OracleMappingType.Varchar2);
        yield return new BulkMapping<Customer>("FAX", static c => c.Fax, OracleMappingType.Varchar2);
        yield return new BulkMapping<Customer>("TIDSSTEMPEL", static c => c.TidsStempel, OracleMappingType.Int64);
        yield return new BulkMapping<Customer>("OPPRETTETAV", static c => c.OpprettetAv, OracleMappingType.Varchar2);
        yield return new BulkMapping<Customer>("OPPRETTETTID", static c => c.OpprettetTid, OracleMappingType.Date);
        yield return new BulkMapping<Customer>("SISTENDRETAV", static c => c.SistEndretAv, OracleMappingType.Varchar2);
        yield return new BulkMapping<Customer>("SISTENDRETTID", static c => c.SistEndretTid, OracleMappingType.Date);
        yield return new BulkMapping<Customer>("DIPSID", static c => c.DipsId, OracleMappingType.Int64);
    }

    private static List<Customer> GetCustomersFromEmbeddedResource()
    {
        using Stream stream = typeof(BulkOperationTests).Assembly.GetManifestResourceStream(
                                  "AdaskoTheBeAsT.Dapper.Oracle.IntegrationTests.BulkSql.customers.json")
                              ?? throw new InvalidOperationException("Embedded resource customers.json was not found.");
        return JsonSerializer.Deserialize<List<Customer>>(stream)
               ?? throw new InvalidOperationException("Failed to deserialize customers.json.");
    }

    private sealed class ModernBulkRow
    {
        public Guid Id { get; set; }

        public Guid? OptionalId { get; set; }

        public short ShortVal { get; set; }

        public float FloatVal { get; set; }

        public double DoubleVal { get; set; }

        public bool NumericFlag { get; set; }

        public bool NativeFlag { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public TimeSpan Duration { get; set; }

        public DateOnly? EventDate { get; set; }

        public TimeOnly? EventTime { get; set; }

        public float[] Embedding32 { get; set; } = [];

        public double[] Embedding64 { get; set; } = [];
    }
}
