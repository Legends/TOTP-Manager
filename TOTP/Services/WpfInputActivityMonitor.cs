using System;
using System.Diagnostics;
using System.Windows.Input;
using TOTP.Infrastructure.Services;
using TOTP.Services.Interfaces;
using TOTP.Views.Interfaces;
using ActivityKind = TOTP.Core.Security.Models.ActivityKind;

namespace TOTP.Services;

/// <summary>
/// Monitors user input activity in a WPF application's main window and notifies an activity service when input events
/// occur.
/// </summary>
/// <remarks>Attach this monitor to a WPF main window to track user interactions such as mouse clicks, mouse wheel
/// movements, key presses, text input, and mouse movement. The monitor throttles mouse move notifications to reduce
/// event frequency. Use this class to enable user activity detection for features such as idle timeout or activity
/// logging. This class is not thread-safe and should be used from the UI thread.</remarks>
public sealed class WpfInputActivityMonitor : IInputActivityMonitor
{
    private static readonly TimeSpan MouseMoveThrottle = TimeSpan.FromMilliseconds(2000);

    private readonly IUserActivityService _activityService;
    private readonly IActivityHeartbeat _heartbeat;
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    private IMainWindow? _window;
    private long _lastMouseMoveTicks;

    public WpfInputActivityMonitor(IActivityHeartbeat heartbeat)
    {
        //_activityService = activityService;
        _heartbeat = heartbeat; // Injected!
        _lastMouseMoveTicks = _stopwatch.ElapsedTicks;
    }

    public void Attach(IMainWindow window)
    {
        if (_window != null)
            return;

        _window = window ?? throw new ArgumentNullException(nameof(window));

        _window.PreviewMouseDown += OnMouseDown;
        _window.PreviewMouseWheel += OnMouseWheel;
        _window.PreviewKeyDown += OnKeyDown;
        _window.PreviewTextInput += OnTextInput;
        _window.PreviewMouseMove += OnMouseMove;
    }

    public void Detach()
    {
        if (_window == null)
            return;

        _window.PreviewMouseDown -= OnMouseDown;
        _window.PreviewMouseWheel -= OnMouseWheel;
        _window.PreviewKeyDown -= OnKeyDown;
        _window.PreviewTextInput -= OnTextInput;
        _window.PreviewMouseMove -= OnMouseMove;

        _window = null;
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (ShouldIgnoreActivity())
            return;

        //_activityService.NotifyActivity(ActivityKind.MouseClick);
        _heartbeat.RecordActivity();
    }

    private void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (ShouldIgnoreActivity())
            return;

        _heartbeat.RecordActivity();
        //_activityService.NotifyActivity(ActivityKind.MouseWheel);
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        Debug.WriteLine($"[Monitor] Key={e.Key}, Handled={e.Handled}, Focus={Keyboard.FocusedElement}");
        if (ShouldIgnoreActivity())
            return;

        _heartbeat.RecordActivity();
    }

    private void OnTextInput(object sender, TextCompositionEventArgs e)
    {
        if (ShouldIgnoreActivity())
            return;

        _heartbeat.RecordActivity();
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (ShouldIgnoreActivity())
            return;

        var elapsedTicks = _stopwatch.ElapsedTicks;
        var delta = TimeSpan.FromTicks(elapsedTicks - _lastMouseMoveTicks);

        if (delta < MouseMoveThrottle)
        {
            //Debug.WriteLine($"Delta: {delta} break;");
            return;
        }
        //Debug.WriteLine($"Delta: {delta} went through;");
        _lastMouseMoveTicks = elapsedTicks;
        _heartbeat.RecordActivity();
    }

    private bool ShouldIgnoreActivity()
    {
        var shouldIgnore = _window == null || !_window.IsActive;

        return shouldIgnore;
    }
}
