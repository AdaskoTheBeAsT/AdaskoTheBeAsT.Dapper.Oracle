using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using AdaskoTheBeAsT.Dapper.Oracle.Tests.Decorators;
using AdaskoTheBeAsT.Dapper.Oracle.Util;
using AwesomeAssertions;
using Oracle.ManagedDataAccess.Client;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.Oracle.Tests;

[SuppressMessage(
    "IDisposableAnalyzers.Correctness",
    "IDISP001:Dispose created",
    Justification = "TheoryData commands are short-lived in-memory command objects for parameter inspection.")]
[SuppressMessage(
    "IDisposableAnalyzers.Correctness",
    "IDISP004:Don't ignore created IDisposable",
    Justification = "Ownership transfers to DecoratedDbCommand primary constructor.")]
[SuppressMessage(
    "Reliability",
    "CA2000:Dispose objects before losing scope",
    Justification = "Ownership transfers into DecoratedDbCommand and TheoryData.")]
public class DecoratedCommandTests
{
    public static TheoryData<IDbCommand, IOracleParameterRetretreiver> Commands()
    {
        return new TheoryData<IDbCommand, IOracleParameterRetretreiver>
        {
            {
                CreateCommand(), new OracleManagedParameterRetretreiver()
            },
            {
                new DecoratedDbCommand(CreateCommand()), new OracleManagedParameterRetretreiver()
            },
            {
                new DecoratedDbCommand(new DecoratedDbCommand(CreateCommand())),
                new OracleManagedParameterRetretreiver()
            },
        };
    }

    [Theory]
    [MemberData(nameof(Commands), DisableDiscoveryEnumeration = true)]
    public void Works_On_Decorated_Commands(
        IDbCommand command,
        IOracleParameterRetretreiver retreiver)
    {
        ThrowHelper.ThrowIfNull(command, nameof(command));
        ThrowHelper.ThrowIfNull(retreiver, nameof(retreiver));

        var parameters = new TestableOracleDynamicParameters();
        parameters.Add("Foo", dbType: OracleMappingType.RefCursor, direction: ParameterDirection.ReturnValue);
        parameters.AddParam(command);
        var oracleParam = retreiver.GetParameter(command.Parameters[0]!);
        oracleParam.OracleDbType.Should().Be("RefCursor");
        oracleParam.Direction.Should().Be(ParameterDirection.ReturnValue);
    }

    [Theory]
    [MemberData(nameof(Commands), DisableDiscoveryEnumeration = true)]
    public void Set_BindByName_On_Decorated_Commands(
        IDbCommand command,
        IOracleParameterRetretreiver retreiver)
    {
        ThrowHelper.ThrowIfNull(command, nameof(command));
        ThrowHelper.ThrowIfNull(retreiver, nameof(retreiver));

        var parameters = new TestableOracleDynamicParameters
        {
            BindByName = true,
        };
        parameters.Add("Foo", dbType: OracleMappingType.RefCursor, direction: ParameterDirection.ReturnValue);
        parameters.AddParam(command);
        var oracleParam = retreiver.GetParameter(command.Parameters[0]!);
        oracleParam.OracleDbType.Should().Be("RefCursor");
        oracleParam.Direction.Should().Be(ParameterDirection.ReturnValue);
    }

    [Fact]
    public void DownCastConnection_And_CreateCommand_Work_On_Decorated_Connections()
    {
        using var innerConnection = new OracleConnection();
        using IDbConnection singleDecorated = new DecoratedConnection(innerConnection);
        using IDbConnection doubleDecorated = new DecoratedConnection((DbConnection)singleDecorated);

        doubleDecorated.DownCastConnection().Should().BeSameAs(innerConnection);

        using var createdCommand = doubleDecorated.CreateCommand();
        createdCommand.Should().BeOfType<DecoratedDbCommand>();
        createdCommand.DownCastCommand().Should().BeOfType<OracleCommand>();
    }

    private static OracleCommand CreateCommand() => new();
}
