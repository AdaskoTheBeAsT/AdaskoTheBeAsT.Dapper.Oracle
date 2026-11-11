using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace AdaskoTheBeAsT.Dapper.Oracle.Tests.Decorators;

[SuppressMessage(
    "IDisposableAnalyzers.Correctness",
    "IDISP007:Don't dispose injected",
    Justification = "Decorator forwards disposal to wrapped command.")]
[SuppressMessage(
    "IDisposableAnalyzers.Correctness",
    "IDISP008:Don't assign member with injected and created disposables",
    Justification = "Decorator exposes wrapped command property.")]
[SuppressMessage(
    "Reliability",
    "CA2213:Disposable fields should be disposed",
    Justification = "Disposed in Dispose(bool).")]
internal sealed class DecoratedDbCommand : DbCommand
{
    public DecoratedDbCommand(DbCommand decorated)
    {
        Decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
    }

    public DbCommand Decorated { get; }

    [AllowNull]
    [SuppressMessage(
        "Security",
        "CA2100:Review SQL queries for security vulnerabilities",
        Justification = "Test command decorator forwards CommandText unchanged.")]
    [SuppressMessage(
        "Security",
        "SCS0002:Potential SQL injection",
        Justification = "Test command decorator forwards CommandText unchanged.")]
    public override string CommandText
    {
        get => Decorated.CommandText;
        set => Decorated.CommandText = value ?? string.Empty;
    }

    public override int CommandTimeout
    {
        get => Decorated.CommandTimeout;
        set => Decorated.CommandTimeout = value;
    }

    public override CommandType CommandType
    {
        get => Decorated.CommandType;
        set => Decorated.CommandType = value;
    }

    public override UpdateRowSource UpdatedRowSource
    {
        get => Decorated.UpdatedRowSource;
        set => Decorated.UpdatedRowSource = value;
    }

    public override bool DesignTimeVisible
    {
        get => Decorated.DesignTimeVisible;
        set => Decorated.DesignTimeVisible = value;
    }

    protected override DbConnection? DbConnection
    {
        get => Decorated.Connection;
        set => Decorated.Connection = value;
    }

    protected override DbParameterCollection DbParameterCollection => Decorated.Parameters;

    protected override DbTransaction? DbTransaction
    {
        get => Decorated.Transaction;
        set => Decorated.Transaction = value;
    }

    public override void Cancel()
    {
        Decorated.Cancel();
    }

    public override int ExecuteNonQuery()
    {
        return Decorated.ExecuteNonQuery();
    }

    public override object? ExecuteScalar()
    {
        return Decorated.ExecuteScalar();
    }

    public override void Prepare()
    {
        Decorated.Prepare();
    }

    protected override DbParameter CreateDbParameter()
    {
        return Decorated.CreateParameter();
    }

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
        return Decorated.ExecuteReader(behavior);
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
