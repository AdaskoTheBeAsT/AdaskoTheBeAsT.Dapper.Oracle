using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace AdaskoTheBeAsT.Dapper.Oracle.Tests.Decorators;

[SuppressMessage(
    "IDisposableAnalyzers.Correctness",
    "IDISP004:Don't ignore created IDisposable",
    Justification = "Ownership transfers to wrapper primary constructor.")]
[SuppressMessage(
    "IDisposableAnalyzers.Correctness",
    "IDISP007:Don't dispose injected",
    Justification = "Decorator forwards disposal to wrapped connection.")]
[SuppressMessage(
    "IDisposableAnalyzers.Correctness",
    "IDISP008:Don't assign member with injected and created disposables",
    Justification = "Decorator exposes wrapped connection property.")]
[SuppressMessage(
    "Reliability",
    "CA2213:Disposable fields should be disposed",
    Justification = "Disposed in Dispose(bool).")]
internal sealed class DecoratedConnection(DbConnection decorated) : DbConnection
{
    public DbConnection Decorated { get; } = decorated ?? throw new ArgumentNullException(nameof(decorated));

#if NETCOREAPP3_0_OR_GREATER
    [AllowNull]
#endif
    public override string ConnectionString
    {
        get => Decorated.ConnectionString;
        set => Decorated.ConnectionString = value ?? string.Empty;
    }

    public override string Database => Decorated.Database;

    public override ConnectionState State => Decorated.State;

    public override string DataSource => Decorated.DataSource;

    public override string ServerVersion => Decorated.ServerVersion;

    public override void ChangeDatabase(string databaseName)
    {
        Decorated.ChangeDatabase(databaseName);
    }

    public override void Close()
    {
        Decorated.Close();
    }

    public override void Open()
    {
        Decorated.Open();
    }

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
        return new DecoratedTransaction(Decorated.BeginTransaction(isolationLevel), this);
    }

    protected override DbCommand CreateDbCommand()
    {
        return new DecoratedDbCommand(Decorated.CreateCommand());
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Decorated.Dispose();
        }

        base.Dispose(disposing);
    }
}
