using Microsoft.Win32;
using TOTP.Core.Platform;
using TOTP.Presentation.Platform;

namespace TOTP.Tests.Services;

public sealed class WindowsPlatformEventSourceTests
{
    [Theory]
    [InlineData(SessionSwitchReason.SessionLock, PlatformSessionState.Locked)]
    [InlineData(SessionSwitchReason.SessionUnlock, PlatformSessionState.Active)]
    [InlineData(SessionSwitchReason.SessionLogon, PlatformSessionState.Active)]
    [InlineData(SessionSwitchReason.ConsoleDisconnect, PlatformSessionState.Disconnected)]
    [InlineData(SessionSwitchReason.RemoteDisconnect, PlatformSessionState.Disconnected)]
    public void MapSessionState_MapsWindowsReason(
        SessionSwitchReason reason,
        PlatformSessionState expected)
    {
        Assert.Equal(expected, WindowsPlatformEventSource.MapSessionState(reason));
    }

    [Fact]
    public void MapSessionState_IgnoresRemoteControlChange()
    {
        Assert.Null(WindowsPlatformEventSource.MapSessionState(SessionSwitchReason.SessionRemoteControl));
    }

    [Theory]
    [InlineData(PowerModes.Suspend, PlatformLifecycleState.Suspending)]
    [InlineData(PowerModes.Resume, PlatformLifecycleState.Resumed)]
    public void MapLifecycleState_MapsPowerMode(PowerModes mode, PlatformLifecycleState expected)
    {
        Assert.Equal(expected, WindowsPlatformEventSource.MapLifecycleState(mode));
    }

    [Fact]
    public void MapLifecycleState_IgnoresStatusChange()
    {
        Assert.Null(WindowsPlatformEventSource.MapLifecycleState(PowerModes.StatusChange));
    }
}
