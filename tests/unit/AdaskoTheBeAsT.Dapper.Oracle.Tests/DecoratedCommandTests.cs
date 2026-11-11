using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using AdaskoTheBeAsT.Dapper.Oracle.Tests.Decorators;
using AwesomeAssertions;
using Oracle.ManagedDataAccess.Client;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.Oracle.Tests;

[SuppressMessage(
    "IDisposableAnalyzers.Correctness",
    "IDISP001:Dispose created",
    Justification = "TheoryData commands are short-lived in-memory command objects for parameter inspection.")]
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
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(retreiver);

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
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(retreiver);

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

    private static OracleCommand CreateCommand() => new();
}
