using System;
using System.Diagnostics;
using System.Threading;
using TOTP.Core.Platform;

namespace TOTP.Presentation.Platform;

public sealed class WindowsNamedMutexInstanceLock : IInstanceLock
{
    private readonly string _mutexName;
    private Mutex? _mutex;
    private bool _ownsMutex;
    private bool _acquireAttempted;
    private bool _disposed;

    public WindowsNamedMutexInstanceLock(string applicationId, bool globalNamespace = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        _mutexName = $@"{(globalNamespace ? "Global" : "Local")}\{applicationId}";
    }

    public InstanceLockAcquireResult Acquire()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_acquireAttempted)
        {
            throw new InvalidOperationException("The instance lock can only be acquired once.");
        }

        _acquireAttempted = true;
        _mutex = new Mutex(initiallyOwned: false, _mutexName, out var createdNew);

        try
        {
            if (!_mutex.WaitOne(0))
            {
                return InstanceLockAcquireResult.AlreadyRunning;
            }

            _ownsMutex = true;
            return createdNew
                ? InstanceLockAcquireResult.Acquired
                : InstanceLockAcquireResult.Recovered;
        }
        catch (AbandonedMutexException)
        {
            _ownsMutex = true;
            return InstanceLockAcquireResult.Recovered;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        var mutex = Interlocked.Exchange(ref _mutex, null);
        if (mutex == null)
        {
            return;
        }

        try
        {
            if (_ownsMutex)
            {
                mutex.ReleaseMutex();
            }
        }
        catch (ApplicationException ex)
        {
            Trace.TraceWarning($"Named mutex release skipped because the current thread does not own it: {ex.Message}");
        }
        finally
        {
            mutex.Dispose();
        }
    }
}
