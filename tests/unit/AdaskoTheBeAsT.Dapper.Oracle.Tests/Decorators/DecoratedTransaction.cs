using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace AdaskoTheBeAsT.Dapper.Oracle.Tests.Decorators;

[SuppressMessage(
    "IDisposableAnalyzers.Correctness",
    "IDISP007:Don't dispose injected",
    Justification = "Decorator forwards disposal to wrapped transaction.")]
[SuppressMessage(
    "IDisposableAnalyzers.Correctness",
    "IDISP008:Don't assign member with injected and created disposables",
    Justification = "Decorator wraps caller-supplied transaction and connection.")]
[SuppressMessage(
    "Reliability",
    "CA2213:Disposable fields should be disposed",
    Justification = "Transaction is disposed in Dispose(bool); connection lifetime belongs to caller.")]
public class DecoratedTransaction : DbTransaction
{
    private DbConnection? _connection;
    private DbTransaction? _transaction;

    public DecoratedTransaction(
        DbTransaction transaction,
        DbConnection connection)
    {
        _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    public DbTransaction? WrappedTransaction => _transaction;

    public override IsolationLevel IsolationLevel => _transaction?.IsolationLevel ?? IsolationLevel.Unspecified;

    protected override DbConnection? DbConnection => _connection;

    public override void Commit()
    {
        _transaction?.Commit();
    }

    public override void Rollback()
    {
        _transaction?.Rollback();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _transaction?.Dispose();
        }

        _transaction = null;
        _connection = null;
        base.Dispose(disposing);
    }
}
