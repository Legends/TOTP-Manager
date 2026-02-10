using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using TOTP.Security.Interfaces;
using TOTP.Services.Interfaces;
using ActivityKind = TOTP.Security.Models.ActivityKind;

namespace TOTP.Services;

public sealed class WpfInputActivityMonitor : IInputActivityMonitor
{
    private static readonly TimeSpan MouseMoveThrottle = TimeSpan.FromMilliseconds(2000);

    private readonly IUserActivityService _activityService;
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    private Window? _window;
    private long _lastMouseMoveTicks;

    public WpfInputActivityMonitor(IUserActivityService activityService)
    {
        _activityService = activityService;
        _lastMouseMoveTicks = _stopwatch.ElapsedTicks;
    }

    public void Attach(Window window)
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

        _activityService.NotifyActivity(ActivityKind.MouseClick);
    }

    private void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (ShouldIgnoreActivity())
            return;

        _activityService.NotifyActivity(ActivityKind.MouseWheel);
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (ShouldIgnoreActivity())
            return;

        _activityService.NotifyActivity(ActivityKind.KeyPress);
    }

    private void OnTextInput(object sender, TextCompositionEventArgs e)
    {
        if (ShouldIgnoreActivity())
            return;

        _activityService.NotifyActivity(ActivityKind.TextInput);
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (ShouldIgnoreActivity())
            return;

        var elapsedTicks = _stopwatch.ElapsedTicks;
        var delta = TimeSpan.FromTicks(elapsedTicks - _lastMouseMoveTicks);
        
        if (delta < MouseMoveThrottle)
        {
            Debug.WriteLine($"Delta: {delta} break;");
            return;
        }
        Debug.WriteLine($"Delta: {delta} went through;");
        _lastMouseMoveTicks = elapsedTicks;
        _activityService.NotifyActivity(ActivityKind.MouseMove);
    }

    private bool ShouldIgnoreActivity()
    {
        var shouldIgnore = _window == null || !_window.IsActive;
        Debug.WriteLine($"Should ignore activity: {shouldIgnore}");
        return shouldIgnore;
    }
}
