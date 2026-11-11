using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Dapper.Oracle.IntegrationTests.Common;
using Oracle.ManagedDataAccess.Client;
using Testcontainers.Oracle;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.Oracle.IntegrationTests;

[SuppressMessage(
    "IDisposableAnalyzers.Correctness",
    "IDISP006:Implement IDisposable",
    Justification = "Disposed via FixtureResourceLifecycle.")]
public sealed class DatabaseFixture
    : IAsyncLifetime,
        IDisposable
{
    private readonly FixtureResourceLifecycle _lifecycle;

    [SuppressMessage(
        "Reliability",
        "CA2213:Disposable fields should be disposed",
        Justification = "FixtureResourceLifecycle invokes CleanupAsync exactly once.")]
    private readonly OracleContainer _oracleContainer =
        new OracleBuilder("gvenzl/oracle-free:23-slim-faststart")
            .WithPassword("TestPass123!")
            .WithExposedPort(1521)
            .Build();

    [SuppressMessage(
        "Reliability",
        "CA2213:Disposable fields should be disposed",
        Justification = "FixtureResourceLifecycle invokes CleanupAsync exactly once.")]
    private OracleConnection? _connection;

    public DatabaseFixture()
    {
        _lifecycle = new FixtureResourceLifecycle(InitializeCoreAsync, CleanupAsync);
    }

    public IDbConnection Connection =>
        _connection ?? throw new InvalidOperationException("DatabaseFixture has not been initialized.");

    public string ConnectionString { get; private set; } = string.Empty;

    public ValueTask InitializeAsync() => new(_lifecycle.InitializeAsync());

    public ValueTask DisposeAsync() => _lifecycle.DisposeAsync();

    public void Dispose() => _lifecycle.Dispose();

    private async Task InitializeCoreAsync()
    {
        var externalConnectionString = Environment.GetEnvironmentVariable("DA_OR_CONNECTION");
        if (!string.IsNullOrWhiteSpace(externalConnectionString))
        {
            ConnectionString = externalConnectionString;
        }
        else
        {
            await _oracleContainer.StartAsync(TestContext.Current.CancellationToken).ConfigureAwait(false);
            ConnectionString = _oracleContainer.GetConnectionString();
        }

        _connection = new OracleConnection(ConnectionString);
        await _connection.OpenAsync(TestContext.Current.CancellationToken).ConfigureAwait(false);
    }

    private async Task CleanupAsync()
    {
        if (_connection is not null)
        {
            await _connection.CloseAsync().ConfigureAwait(false);
            await _connection.DisposeAsync().ConfigureAwait(false);
            _connection = null;
        }

        await _oracleContainer.DisposeAsync().ConfigureAwait(false);
    }
}
