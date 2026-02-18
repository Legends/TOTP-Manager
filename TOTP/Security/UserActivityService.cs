using System;
using System.Diagnostics;
using System.Windows.Threading;
using TOTP.Security.Interfaces;
using TOTP.Security.Models;
using ActivityKind = TOTP.Security.Models.ActivityKind;
namespace TOTP.Security;


/// <summary>
/// Provides functionality to monitor user activity and detect periods of inactivity, raising an event when the user is
/// considered idle.
/// </summary>
/// <remarks>This service tracks user activity and triggers the <see cref="LockRequested"/> event when the
/// configured idle timeout is exceeded. It is typically used to implement automatic locking or session timeout features
/// in applications. The service must be explicitly started and stopped using <see cref="StartMonitoring"/> and <see
/// cref="StopMonitoring"/>. Thread safety is not guaranteed; all interactions should occur on the UI thread if used
/// with UI components.</remarks>
public sealed class UserActivityService : IUserActivityService
{
    private static readonly TimeSpan DefaultTickInterval = TimeSpan.FromSeconds(1);

    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private readonly DispatcherTimer _timer;
    private readonly TimeSpan _idleTimeout;

    private long _lastActivityTicks;
    private bool _isMonitoring;
    private bool _lockRequested;

    public event EventHandler? LockRequested;

    public UserActivityService(IGlobalProfileStore profileStore)
    {
        var profile = profileStore.LoadAsync().GetAwaiter().GetResult();
        _idleTimeout = profile?.IdleTimeout ?? GlobalProfile.DefaultIdleTimeout;
        _lastActivityTicks = _stopwatch.ElapsedTicks;

        if (_idleTimeout <= TimeSpan.Zero)
            _idleTimeout = GlobalProfile.DefaultIdleTimeout; // 10min

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
