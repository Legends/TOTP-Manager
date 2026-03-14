using System.Diagnostics;
using TOTP.Infrastructure.Services;
using TOTP.Services;
using TOTP.Views.Interfaces;

namespace TOTP.Tests.Services;

public sealed class WpfInputActivityMonitorTests
{
    [Fact]
    public void MouseDown_WhenWindowActive_RecordsActivity()
    {
        var heartbeat = new TestHeartbeat();
        var sut = new WpfInputActivityMonitor(heartbeat);
        var window = new TestMainWindow { IsActive = true };
        sut.Attach(window);

        window.RaiseMouseDown();

        Assert.Equal(1, heartbeat.RecordCount);
    }

    [Fact]
    public void MouseDown_WhenWindowInactive_DoesNotRecordActivity()
    {
        var heartbeat = new TestHeartbeat();
        var sut = new WpfInputActivityMonitor(heartbeat);
        var window = new TestMainWindow { IsActive = false };
        sut.Attach(window);

        window.RaiseMouseDown();

        Assert.Equal(0, heartbeat.RecordCount);
    }

    [Fact]
    public void Detach_StopsTrackingInputEvents()
    {
        var heartbeat = new TestHeartbeat();
        var sut = new WpfInputActivityMonitor(heartbeat);
        var window = new TestMainWindow { IsActive = true };
        sut.Attach(window);
        sut.Detach();

        window.RaiseMouseWheel();
        window.RaiseTextInput();

        Assert.Equal(0, heartbeat.RecordCount);
    }

    [Fact]
    public void MouseMove_IsThrottled()
    {
        var heartbeat = new TestHeartbeat();
        var sut = new WpfInputActivityMonitor(heartbeat);
        var window = new TestMainWindow { IsActive = true };
        sut.Attach(window);

        var stopwatch = (Stopwatch)typeof(WpfInputActivityMonitor)
            .GetField("_stopwatch", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .GetValue(sut)!;
        var backdated = stopwatch.ElapsedTicks - (Stopwatch.Frequency * 3);
        typeof(WpfInputActivityMonitor)
            .GetField("_lastMouseMoveTicks", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(sut, backdated);

        window.RaiseMouseMove();
        window.RaiseMouseMove();

        Assert.Equal(1, heartbeat.RecordCount);
    }

    private sealed class TestHeartbeat : IActivityHeartbeat
    {
        public int RecordCount { get; private set; }
        public DateTime LastActivity { get; private set; } = DateTime.UtcNow;

        public void RecordActivity()
        {
            RecordCount++;
            LastActivity = DateTime.UtcNow;
        }
    }

    private sealed class TestMainWindow : IMainWindow
    {
        public bool IsActive { get; set; }

        public void BringToFront()
        {
        }

        public event System.Windows.Input.MouseButtonEventHandler? PreviewMouseDown;
        public event System.Windows.Input.MouseWheelEventHandler? PreviewMouseWheel;
        public event System.Windows.Input.KeyEventHandler? PreviewKeyDown;
        public event System.Windows.Input.TextCompositionEventHandler? PreviewTextInput;
        public event System.Windows.Input.MouseEventHandler? PreviewMouseMove;

        public void RaiseMouseDown() => PreviewMouseDown?.Invoke(this, null!);
        public void RaiseMouseWheel() => PreviewMouseWheel?.Invoke(this, null!);
        public void RaiseKeyDown() => PreviewKeyDown?.Invoke(this, null!);
        public void RaiseTextInput() => PreviewTextInput?.Invoke(this, null!);
        public void RaiseMouseMove() => PreviewMouseMove?.Invoke(this, null!);
    }
}
