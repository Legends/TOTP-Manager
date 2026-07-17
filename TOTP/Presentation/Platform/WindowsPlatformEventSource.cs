using System;
using Microsoft.Win32;
using TOTP.Core.Platform;

namespace TOTP.Presentation.Platform;

public sealed class WindowsPlatformEventSource : IPlatformSessionEventSource, IPlatformLifecycleEventSource
{
    private readonly object _sync = new();
    private bool _sessionStarted;
    private bool _lifecycleStarted;

    public bool IsSupported => OperatingSystem.IsWindows();

    public event EventHandler<PlatformSessionChangedEventArgs>? SessionChanged;
    public event EventHandler<PlatformLifecycleChangedEventArgs>? LifecycleChanged;

    void IPlatformSessionEventSource.Start()
    {
        lock (_sync)
        {
            if (_sessionStarted || !IsSupported)
            {
                return;
            }

            SystemEvents.SessionSwitch += OnSessionSwitch;
            _sessionStarted = true;
        }
    }

    void IPlatformSessionEventSource.Stop()
    {
        lock (_sync)
        {
            if (!_sessionStarted)
            {
                return;
            }

            SystemEvents.SessionSwitch -= OnSessionSwitch;
            _sessionStarted = false;
        }
    }

    void IPlatformLifecycleEventSource.Start()
    {
        lock (_sync)
        {
            if (_lifecycleStarted || !IsSupported)
            {
                return;
            }

            SystemEvents.PowerModeChanged += OnPowerModeChanged;
            _lifecycleStarted = true;
        }
    }

    void IPlatformLifecycleEventSource.Stop()
    {
        lock (_sync)
        {
            if (!_lifecycleStarted)
            {
                return;
            }

            SystemEvents.PowerModeChanged -= OnPowerModeChanged;
            _lifecycleStarted = false;
        }
    }

    private void OnSessionSwitch(object sender, SessionSwitchEventArgs args)
    {
        var state = MapSessionState(args.Reason);
        if (state.HasValue)
        {
            SessionChanged?.Invoke(this, new PlatformSessionChangedEventArgs(state.Value));
        }
    }

    private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs args)
    {
        var state = MapLifecycleState(args.Mode);
        if (state.HasValue)
        {
            LifecycleChanged?.Invoke(this, new PlatformLifecycleChangedEventArgs(state.Value));
        }
    }

    internal static PlatformSessionState? MapSessionState(SessionSwitchReason reason) => reason switch
    {
        SessionSwitchReason.SessionLock => PlatformSessionState.Locked,
        SessionSwitchReason.SessionUnlock or
        SessionSwitchReason.SessionLogon or
        SessionSwitchReason.ConsoleConnect or
        SessionSwitchReason.RemoteConnect => PlatformSessionState.Active,
        SessionSwitchReason.SessionLogoff or
        SessionSwitchReason.ConsoleDisconnect or
        SessionSwitchReason.RemoteDisconnect => PlatformSessionState.Disconnected,
        _ => null
    };

    internal static PlatformLifecycleState? MapLifecycleState(PowerModes mode) => mode switch
    {
        PowerModes.Suspend => PlatformLifecycleState.Suspending,
        PowerModes.Resume => PlatformLifecycleState.Resumed,
        _ => null
    };
}
