using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace AdaskoTheBeAsT.Dapper.Oracle.IntegrationTests.Common;

/// <summary>Owns one initialization attempt and one cleanup attempt, including failed initialization.</summary>
internal sealed class FixtureResourceLifecycle(
    Func<Task> initialize,
    Func<Task> cleanup)
    : IAsyncDisposable,
        IDisposable
{
    internal const string CleanupFailureKey = "FixtureCleanupException";
#if NET9_0_OR_GREATER
    private readonly Lock _gate = new();
#else
    private readonly object _gate = new();
#endif
    private Task? _initializationTask;
    private Task? _cleanupTask;
    private Task? _disposalTask;

    public Task InitializeAsync()
    {
        lock (_gate)
        {
            if (_disposalTask != null)
            {
                return Task.FromException(new ObjectDisposedException(nameof(FixtureResourceLifecycle)));
            }

            // Cache failures too: never retry against a partially initialized resource.
            return _initializationTask ??= InitializeCoreAsync();
        }
    }

    public ValueTask DisposeAsync()
    {
        lock (_gate)
        {
            return new ValueTask(_disposalTask ??= DisposeCoreAsync(_initializationTask));
        }
    }

#pragma warning disable VSTHRD002 // Required synchronous IDisposable bridge; lifecycle awaits do not capture context.
    public void Dispose() => DisposeAsync().AsTask().GetAwaiter().GetResult();
#pragma warning restore VSTHRD002

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Preserves initialization exception while attempting cleanup.")]
    private async Task InitializeCoreAsync()
    {
        try
        {
            await (initialize?.Invoke() ?? throw new ArgumentNullException(nameof(initialize))).ConfigureAwait(false);
        }
        catch (Exception initializationFailure)
        {
            try
            {
                await CleanupOnceAsync().ConfigureAwait(false);
            }
            catch (Exception cleanupFailure)
            {
                // Keep the original exception/stack while retaining the cleanup failure for diagnosis.
                initializationFailure.Data[CleanupFailureKey] = cleanupFailure;
            }

            throw;
        }
    }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Initialization reports its own failure; disposal must still observe cleanup.")]
    private async Task DisposeCoreAsync(Task? initialization)
    {
        if (initialization != null)
        {
            try
            {
#pragma warning disable VSTHRD003 // Observe the cached initialization task; this fixture has no UI context.
                await initialization.ConfigureAwait(false);
#pragma warning restore VSTHRD003
            }
            catch (Exception)
            {
                await CleanupOnceAsync().ConfigureAwait(false);
                return;
            }
        }

        await CleanupOnceAsync().ConfigureAwait(false);
    }

    private Task CleanupOnceAsync()
    {
        lock (_gate)
        {
            return _cleanupTask ??= CleanupCoreAsync();
        }
    }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Caches synchronous callback failures just like asynchronous cleanup failures.")]
#pragma warning disable MA0038, RCS1229 // Uses the captured constructor callback and caches synchronous failures without an async wrapper.
    private Task CleanupCoreAsync()
    {
        try
        {
            return cleanup?.Invoke() ?? throw new ArgumentNullException(nameof(cleanup));
        }
        catch (Exception exception)
        {
            return Task.FromException(exception);
        }
    }
#pragma warning restore MA0038, RCS1229
}
