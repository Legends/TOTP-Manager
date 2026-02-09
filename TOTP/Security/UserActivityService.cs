using System;
using System.Diagnostics;
using System.Windows.Threading;

namespace TOTP.Security;

public sealed class UserActivityService : IUserActivityService
{
    private static readonly TimeSpan DefaultIdleTimeout = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan DefaultTickInterval = TimeSpan.FromSeconds(1);

    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private readonly DispatcherTimer _timer;
    private readonly TimeSpan _idleTimeout;

    private long _lastActivityTicks;
    private bool _isMonitoring;
    private bool _lockRequested;

    public event EventHandler? LockRequested;

    public UserActivityService()
    {
        _idleTimeout = DefaultIdleTimeout;
        _lastActivityTicks = _stopwatch.ElapsedTicks;

        _timer = new DispatcherTimer
        {
            Interval = DefaultTickInterval
        };
        _timer.Tick += (_, _) => CheckIdle();
    }

    public TimeSpan TimeSinceLastActivity
        => _isMonitoring
            ? TimeSpan.FromTicks(_stopwatch.ElapsedTicks - _lastActivityTicks)
            : TimeSpan.Zero;

    public void NotifyActivity(ActivityKind kind)
    {
        if (!_isMonitoring)
            return;

        _lastActivityTicks = _stopwatch.ElapsedTicks;
        _lockRequested = false;
    }

    public void StartMonitoring()
    {
        if (_isMonitoring)
            return;

        _isMonitoring = true;
        _lastActivityTicks = _stopwatch.ElapsedTicks;
        _lockRequested = false;
        _timer.Start();
    }

    public void StopMonitoring()
    {
        if (!_isMonitoring)
            return;

        _isMonitoring = false;
        _timer.Stop();
    }

    private void CheckIdle()
    {
        if (!_isMonitoring || _lockRequested)
            return;

        if (TimeSinceLastActivity < _idleTimeout)
            return;

        _lockRequested = true;
        LockRequested?.Invoke(this, EventArgs.Empty);
    }
}
