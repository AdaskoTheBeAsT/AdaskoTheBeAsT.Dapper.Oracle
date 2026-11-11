using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace AdaskoTheBeAsT.Dapper.Oracle.Tests.Decorators;

[SuppressMessage(
    "IDisposableAnalyzers.Correctness",
    "IDISP007:Don't dispose injected",
    Justification = "Decorator forwards disposal to wrapped connection.")]
[SuppressMessage(
    "Reliability",
    "CA2213:Disposable fields should be disposed",
    Justification = "Disposed in Dispose(bool).")]
internal sealed class DecoratedConnection : DbConnection
{
    private readonly DbConnection _decorated;

    public DecoratedConnection(DbConnection decorated)
    {
        _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
    }

    [AllowNull]
    public override string ConnectionString
    {
        get => _decorated.ConnectionString;
        set => _decorated.ConnectionString = value ?? string.Empty;
    }

    public override string Database => _decorated.Database;

    public override ConnectionState State => _decorated.State;

    public override string DataSource => _decorated.DataSource;

    public override string ServerVersion => _decorated.ServerVersion;

    public override void ChangeDatabase(string databaseName)
    {
        _decorated.ChangeDatabase(databaseName);
    }

    public override void Close()
    {
        _decorated.Close();
    }

    public override void Open()
    {
        _decorated.Open();
    }

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
        return new DecoratedTransaction(_decorated.BeginTransaction(isolationLevel), this);
    }

    protected override DbCommand CreateDbCommand()
    {
        return new DecoratedDbCommand(_decorated.CreateCommand());
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _decorated.Dispose();
        }

        base.Dispose(disposing);
    }
}
