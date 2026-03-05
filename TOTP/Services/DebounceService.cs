using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TOTP.Services.Interfaces;

namespace TOTP.Services;

public sealed class DebounceService : IDebounceService
{
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _tokens = new();
    private int _isDisposed;

    public void Debounce(string key, int milliseconds, Action action)
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Debounce key cannot be null or whitespace.", nameof(key));
        }

        if (milliseconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(milliseconds), "Debounce delay must be non-negative.");
        }

        if (_tokens.TryRemove(key, out var existingCts))
        {
            existingCts.Cancel();
            existingCts.Dispose();
        }

        var cts = new CancellationTokenSource();
        _tokens[key] = cts;
        _ = RunDebouncedAsync(key, milliseconds, action, cts.Token);
    }

    public void Cancel(string key)
    {
        ThrowIfDisposed();

        if (_tokens.TryRemove(key, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _isDisposed, 1) != 0)
        {
            return;
        }

        foreach (var key in _tokens.Keys)
        {
            if (_tokens.TryRemove(key, out var cts))
            {
                cts.Cancel();
                cts.Dispose();
            }
        }
    }

    private async Task RunDebouncedAsync(string key, int milliseconds, Action action, CancellationToken token)
    {
        try
        {
            await Task.Delay(milliseconds, token);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        try
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.HasShutdownStarted && !dispatcher.HasShutdownFinished)
            {
                if (dispatcher.CheckAccess())
                {
                    action();
                }
                else
                {
                    await dispatcher.InvokeAsync(action);
                }
            }
            else
            {
                action();
            }
        }
        finally
        {
            if (_tokens.TryGetValue(key, out var current) && current.Token == token)
            {
                _tokens.TryRemove(key, out var removed);
                removed?.Dispose();
            }
        }
    }

    private void ThrowIfDisposed()
    {
        if (Volatile.Read(ref _isDisposed) != 0)
        {
            throw new ObjectDisposedException(nameof(DebounceService));
        }
    }
}
