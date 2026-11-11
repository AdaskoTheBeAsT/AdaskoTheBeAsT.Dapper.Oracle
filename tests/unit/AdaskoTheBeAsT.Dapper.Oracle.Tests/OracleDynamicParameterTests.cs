using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AwesomeAssertions;
using Oracle.ManagedDataAccess.Client;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.Oracle.Tests;

[SuppressMessage(
    "IDisposableAnalyzers.Correctness",
    "IDISP001:Dispose created",
    Justification = "TheoryData commands are short-lived in-memory command objects for parameter inspection.")]
public class OracleDynamicParameterTests
{
    private readonly TestableOracleDynamicParameters _testObject = [];

    public static TheoryData<IDbCommand, IOracleParameterRetretreiver> OracleDataFixture =>
        new()
        {
            {
                new OracleCommand(), new OracleManagedParameterRetretreiver()
            },
        };

    public static TheoryData<IDbCommand> OracleCommandFixture =>
    [
        new OracleCommand(),
    ];

    [Theory]
    [MemberData(nameof(OracleDataFixture), DisableDiscoveryEnumeration = true)]
    public void SetOracleParameter(
        IDbCommand cmd,
        IOracleParameterRetretreiver retreiver)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        ArgumentNullException.ThrowIfNull(retreiver);

        _testObject.Add("Foo", dbType: OracleMappingType.RefCursor, direction: ParameterDirection.ReturnValue);
        _testObject.AddParam(cmd);
        cmd.Parameters.Count.Should().Be(1);
        var param = retreiver.GetParameter(cmd.Parameters[0]!);
        param.OracleDbType.Should().Be("RefCursor");
        param.ParameterName.Should().Be("Foo");
    }

    [Theory]
    [MemberData(nameof(OracleDataFixture), DisableDiscoveryEnumeration = true)]
    public void SetOracleParameterCommand(
        IDbCommand cmd,
        IOracleParameterRetretreiver retreiver)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        ArgumentNullException.ThrowIfNull(retreiver);

        _testObject.Add("Foo", dbType: OracleMappingType.RefCursor, direction: ParameterDirection.ReturnValue);

        _testObject.AddParam(cmd);
        cmd.Parameters.Count.Should().Be(1);
        var param = retreiver.GetParameter(cmd.Parameters[0]!);
        param.ParameterName.Should().Be("Foo");
        param.OracleDbType.Should().Be("RefCursor");
    }

    [Theory]
    [MemberData(nameof(OracleDataFixture), DisableDiscoveryEnumeration = true)]
    public void SetOracleParameterCommandPlSqlAssociativeArray(
        IDbCommand cmd,
        IOracleParameterRetretreiver retreiver)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        ArgumentNullException.ThrowIfNull(retreiver);

        _testObject.Add("Foo", collectionType: OracleMappingCollectionType.PLSQLAssociativeArray);

        _testObject.AddParam(cmd);
        cmd.Parameters.Count.Should().Be(1);
        var param = retreiver.GetParameter(cmd.Parameters[0]!);
        param.CollectionType.Should().Be("PLSQLAssociativeArray");
    }

    [Theory]
    [MemberData(nameof(OracleDataFixture), DisableDiscoveryEnumeration = true)]
    public void SetAllProperties(
        IDbCommand cmd,
        IOracleParameterRetretreiver retreiver)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        ArgumentNullException.ThrowIfNull(retreiver);

        _testObject.Add(
            "Foo",
            "Bar",
            OracleMappingType.Varchar2,
            ParameterDirection.Input,
            42,
            isNullable: true,
            0,
            0,
            "MySource",
            DataRowVersion.Original);

        _testObject.AddParam(cmd);
        cmd.Parameters.Count.Should().Be(1);
        var param = retreiver.GetParameter(cmd.Parameters[0]!);
        param.ParameterName.Should().Be("Foo");
        param.Value.Should().Be("Bar");
        param.OracleDbType.Should().Be("Varchar2");
        param.Direction.Should().Be(ParameterDirection.Input);
        param.Size.Should().Be(42);
        param.IsNullable.Should().BeTrue();
        param.Scale.Should().Be(0);
        param.Precision.Should().Be(0);
        param.SourceColumn.Should().Be("MySource");
        param.SourceVersion.Should().Be(DataRowVersion.Original);
    }

    [Theory]
    [MemberData(nameof(OracleCommandFixture), DisableDiscoveryEnumeration = true)]
    public void SetBindByNameFalse(IDbCommand cmd)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        _testObject.BindByName = true;
        _testObject.Add("Foo", "Bar");
        _testObject.AddParam(cmd);

        ((OracleCommand)cmd).BindByName.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(OracleCommandFixture), DisableDiscoveryEnumeration = true)]
    public void SetBindByNameNotSet(IDbCommand cmd)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        _testObject.BindByName = false;
        _testObject.Add("Foo", "Bar");
        _testObject.AddParam(cmd);

        ((OracleCommand)cmd).BindByName.Should().BeFalse();
    }

    [Fact]
    public void WrongTypeConnectionWillThrowException()
    {
        using var cmd = new FakeDbCommand();
        _testObject.Add("Foo", "Bar");
        var act = () => _testObject.AddParam(cmd);
        act.Should().Throw<NotSupportedException>();
    }

    [Theory]
    [MemberData(nameof(OracleDataFixture), DisableDiscoveryEnumeration = true)]
    public void ArrayBindSizeSet_ValueIsSet(
        IDbCommand cmd,
        IOracleParameterRetretreiver retreiver)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        ArgumentNullException.ThrowIfNull(retreiver);

        var bindSizeArray = Enumerable.Range(0, 20).ToArray();
        _testObject.Add("Foo", "Bar", arrayBindSize: bindSizeArray);

        _testObject.AddParam(cmd);

        cmd.Parameters.Count.Should().Be(1);
        var param = retreiver.GetParameter(cmd.Parameters[0]!);
        param.ParameterName.Should().Be("Foo");
        param.Value.Should().Be("Bar");
        param.ArrayBindSize.Should().BeSameAs(bindSizeArray);
    }

    [Theory]
    [MemberData(nameof(OracleCommandFixture), DisableDiscoveryEnumeration = true)]
    public void GetParameter_DoesNotReturnNull(IDbCommand cmd)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var bindSizeArray = Enumerable.Range(0, 20).ToArray();
        _testObject.Add("Foo", "Bar", arrayBindSize: bindSizeArray);

        _testObject.AddParam(cmd);

        cmd.Parameters.Count.Should().Be(1);
        var param = _testObject.GetParameter("Foo");
        param.Name.Should().Be("Foo");
        param.Value.Should().Be("Bar");
        param.ArrayBindSize.Should().BeSameAs(bindSizeArray);
    }

    [Fact]
    public void GetParameterValue()
    {
        _testObject.Add("Foo", "Bar", OracleMappingType.Varchar2);

        _testObject.Get<string>("Foo").Should().Be("Bar");
    }

    [Fact]
    public void GetParameter()
    {
        _testObject.Add("Foo", "Bar", OracleMappingType.Varchar2);

        var param = _testObject.GetParameter("Foo");

        param.Should().NotBeNull();
        param.Name.Should().Be("Foo");
        param.Value.Should().Be("Bar");
        param.DbType.Should().Be(OracleMappingType.Varchar2);
    }

    [Fact]
    public void CollectionInitializer()
    {
        var parameters = new OracleDynamicParameters
        {
            {
                "param1", 42, OracleMappingType.Int32, ParameterDirection.Input
            },
            {
                "param2", "test", OracleMappingType.Varchar2, ParameterDirection.Input
            },
        };

        parameters.ParameterNames.Should().Equal("param1", "param2");
    }

    [Fact]
    public void CommandTuningProperties_SupportOracleRecommendations()
    {
        using var cmd = new OracleCommand();
        var parameters = new TestableOracleDynamicParameters
        {
            InitialLOBFetchSize = -1,
            InitialLONGFetchSize = -1,
            FetchSize = 131_072L,
            AddToStatementCache = false,
            BindByName = true,
        };
        parameters.Add("Payload", "{}", OracleMappingType.Json);

        parameters.AddParam(cmd);

        cmd.InitialLOBFetchSize.Should().Be(-1);
        cmd.InitialLONGFetchSize.Should().Be(-1);
        cmd.FetchSize.Should().Be(131_072L);
        cmd.AddToStatementCache.Should().BeFalse();
        cmd.BindByName.Should().BeTrue();
    }

    [Fact]
    public void BindByNameWhenUnset_PreservesExistingCommandSetting()
    {
        using var cmd = new OracleCommand
        {
            BindByName = true,
        };
        var parameters = new TestableOracleDynamicParameters
        {
            {
                "Id", 1, OracleMappingType.Int32
            },
        };

        parameters.AddParam(cmd);

        cmd.BindByName.Should().BeTrue();
    }

    [Theory]
    [InlineData(OracleMappingType.Array, OracleDbType.Array)]
    [InlineData(OracleMappingType.Object, OracleDbType.Object)]
    [InlineData(OracleMappingType.Ref, OracleDbType.Ref)]
    [InlineData(OracleMappingType.Boolean, OracleDbType.Boolean)]
    [InlineData(OracleMappingType.Json, OracleDbType.Json)]
    [InlineData(OracleMappingType.ArrayAsJson, OracleDbType.ArrayAsJson)]
    [InlineData(OracleMappingType.ObjectAsJson, OracleDbType.ObjectAsJson)]
    [InlineData(OracleMappingType.Vector, OracleDbType.Vector)]
    [InlineData(OracleMappingType.VectorInt8, OracleDbType.Vector_Int8)]
    [InlineData(OracleMappingType.VectorFloat32, OracleDbType.Vector_Float32)]
    [InlineData(OracleMappingType.VectorFloat64, OracleDbType.Vector_Float64)]
    [InlineData(OracleMappingType.VectorBinary, OracleDbType.Vector_Binary)]
    public void Oracle23aiMappingTypes_MapToExpectedOdpNetOracleDbType(
        OracleMappingType mappingType,
        OracleDbType expectedDbType)
    {
        using var cmd = new OracleCommand();
        var parameters = new TestableOracleDynamicParameters();
        parameters.Add("P1", dbType: mappingType);

        parameters.AddParam(cmd);

        cmd.Parameters[0].OracleDbType.Should().Be(expectedDbType);
    }

    [Fact]
    public void DefaultParameterSizing_HandlesStandardExtendedLobAndOutputBuffers()
    {
        using var cmd = new OracleCommand();
        var parameters = new TestableOracleDynamicParameters
        {
            {
                "ShortVarchar", "abc", OracleMappingType.Varchar2
            },
            {
                "ExtendedVarchar", new string('x', 5000), OracleMappingType.Varchar2
            },
            {
                "ShortNVarchar", "abc", OracleMappingType.NVarchar2
            },
            {
                "ExtendedNVarchar", new string('x', 3000), OracleMappingType.NVarchar2
            },
            {
                "ClobText", new string('c', 10000), OracleMappingType.Clob
            },
            {
                "JsonDoc", "{\"ok\":true}", OracleMappingType.Json
            },
        };
        parameters.Add("OutText", dbType: OracleMappingType.Varchar2, direction: ParameterDirection.Output);
        parameters.Add("OutNText", dbType: OracleMappingType.NVarchar2, direction: ParameterDirection.Output);
        parameters.Add("OutRaw", dbType: OracleMappingType.Raw, direction: ParameterDirection.Output);

        parameters.AddParam(cmd);

        cmd.Parameters["ShortVarchar"].Size.Should().Be(4000);
        cmd.Parameters["ExtendedVarchar"].Size.Should().Be(32767);
        cmd.Parameters["ShortNVarchar"].Size.Should().Be(2000);
        cmd.Parameters["ExtendedNVarchar"].Size.Should().Be(16383);
        cmd.Parameters["ClobText"].Size.Should().Be(0);
        cmd.Parameters["JsonDoc"].Size.Should().Be(0);
        cmd.Parameters["OutText"].Size.Should().Be(4000);
        cmd.Parameters["OutNText"].Size.Should().Be(2000);
        cmd.Parameters["OutRaw"].Size.Should().Be(2000);
    }
}
