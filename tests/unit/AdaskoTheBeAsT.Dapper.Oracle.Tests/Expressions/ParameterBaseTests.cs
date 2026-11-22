using System.Data;
using System.Diagnostics.CodeAnalysis;
using Oracle.ManagedDataAccess.Client;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.Oracle.Tests.Expressions;

[SuppressMessage(
    "IDisposableAnalyzers.Correctness",
    "IDISP001:Dispose created",
    Justification = "TheoryData commands are short-lived in-memory command objects for parameter inspection.")]
public abstract class ParameterBaseTests
{
    public static TheoryData<IDbCommand> OracleDataFixture =>
    [
        new OracleCommand(),
    ];
}
