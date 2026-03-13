using Microsoft.Extensions.Logging;
using NetSparkleUpdater;
using NetSparkleUpdater.Interfaces;
using NetSparkleUpdater.UI.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace TOTP.AutoUpdate;

internal sealed class TOTPNetSparkleUiFactory : IUIFactory
{
    private sealed record WindowParkingState(double Left, double Top, WindowStartupLocation StartupLocation);

    private readonly UIFactory _innerFactory = new();
    private readonly Func<AppCastItem, string?, Task<bool>>? _customInstallHandler;
    private readonly ILogger<TOTPDownloadProgressWindow>? _progressWindowLogger;
    private readonly HashSet<Window> _visibleUpdaterWindows = [];
    private readonly Dictionary<Window, WindowParkingState> _suppressedApplicationWindows = [];
    private readonly DispatcherTimer _restoreWindowsTimer;
    private TOTPDownloadProgressWindow? _activeProgressWindow;

    public TOTPNetSparkleUiFactory(
        Func<AppCastItem, string?, Task<bool>>? customInstallHandler = null,
        ILogger<TOTPDownloadProgressWindow>? progressWindowLogger = null)
    {
        _customInstallHandler = customInstallHandler;
        _progressWindowLogger = progressWindowLogger;
        _restoreWindowsTimer = new DispatcherTimer(
            TimeSpan.FromMilliseconds(750),
            DispatcherPriority.Background,
            (_, _) => RestoreApplicationWindowsIfIdle(),
            Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher);
        _restoreWindowsTimer.Stop();
    }

    public bool HideReleaseNotes
    {
        get => _innerFactory.HideReleaseNotes;
        set => _innerFactory.HideReleaseNotes = value;
    }

    public bool HideSkipButton
    {
        get => _innerFactory.HideSkipButton;
        set => _innerFactory.HideSkipButton = value;
    }

    public bool HideRemindMeLaterButton
    {
        get => _innerFactory.HideRemindMeLaterButton;
        set => _innerFactory.HideRemindMeLaterButton = value;
    }

    public string? ReleaseNotesHTMLTemplate
    {
        get => _innerFactory.ReleaseNotesHTMLTemplate;
        set => _innerFactory.ReleaseNotesHTMLTemplate = value;
    }

    public string? AdditionalReleaseNotesHeaderHTML
    {
        get => _innerFactory.AdditionalReleaseNotesHeaderHTML;
        set => _innerFactory.AdditionalReleaseNotesHeaderHTML = value;
    }

    public IUpdateAvailable CreateUpdateAvailableWindow(SparkleUpdater sparkle, List<AppCastItem> updates, bool isUpdateAlreadyDownloaded)
    {
        return InvokeOnUi(() => PrepareWindow(new TOTPUpdateAvailableWindow(updates, isUpdateAlreadyDownloaded)));
    }

    public IDownloadProgress CreateProgressWindow(SparkleUpdater sparkle, AppCastItem item)
    {
        return InvokeOnUi(() =>
        {
            var window = PrepareWindow(new TOTPDownloadProgressWindow(item, _customInstallHandler, _progressWindowLogger));
            window.Closed += (_, _) =>
            {
                if (ReferenceEquals(_activeProgressWindow, window))
                {
                    _activeProgressWindow = null;
                }
            };

            _activeProgressWindow = window;
            return _activeProgressWindow;
        });
    }

    public ICheckingForUpdates ShowCheckingForUpdates(SparkleUpdater sparkle)
    {
        return InvokeOnUi(() => PrepareWindow(new TOTPCheckingForUpdatesWindow()));
    }

    public void Init(SparkleUpdater sparkle)
    {
        _innerFactory.Init(sparkle);
    }

    public void ShowUnknownInstallerFormatMessage(SparkleUpdater sparkle, string downloadFileName)
    {
        _innerFactory.ShowUnknownInstallerFormatMessage(sparkle, downloadFileName);
    }

    public void ShowVersionIsUpToDate(SparkleUpdater sparkle)
    {
        _innerFactory.ShowVersionIsUpToDate(sparkle);
    }

    public void ShowVersionIsSkippedByUserRequest(SparkleUpdater sparkle)
    {
        _innerFactory.ShowVersionIsSkippedByUserRequest(sparkle);
    }

    public void ShowCannotDownloadAppcast(SparkleUpdater sparkle, string appcastUrl)
    {
        _innerFactory.ShowCannotDownloadAppcast(sparkle, appcastUrl);
    }

    public bool CanShowToastMessages(SparkleUpdater sparkle)
    {
        return _innerFactory.CanShowToastMessages(sparkle);
    }

    public void ShowToast(SparkleUpdater sparkle, List<AppCastItem> updates, Action<List<AppCastItem>> clickHandler)
    {
        _innerFactory.ShowToast(sparkle, updates, clickHandler);
    }

    public void ShowDownloadErrorMessage(SparkleUpdater sparkle, string message, string appcastUrl)
    {
        if (_activeProgressWindow?.DisplayErrorMessage(message) == true)
        {
            return;
        }

        _innerFactory.ShowDownloadErrorMessage(sparkle, message, appcastUrl);
    }

    public void Shutdown(SparkleUpdater sparkle)
    {
        _innerFactory.Shutdown(sparkle);
    }

    public void SetDownloadedFilePath(AppCastItem item, string? downloadedFilePath)
    {
        if (_activeProgressWindow == null)
        {
            return;
        }

        if (!ReferenceEquals(_activeProgressWindow, null))
        {
            _activeProgressWindow.SetDownloadedFilePath(downloadedFilePath);
        }
    }

    private static T InvokeOnUi<T>(Func<T> factory)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher == null || dispatcher.CheckAccess())
        {
            return factory();
        }

        return dispatcher.Invoke(factory);
    }

    private T PrepareWindow<T>(T window) where T : Window
    {
        window.IsVisibleChanged += UpdaterWindow_IsVisibleChanged;
        window.Closed += UpdaterWindow_Closed;
        return window;
    }

    private void UpdaterWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not Window window)
        {
            return;
        }

        if (window.IsVisible)
        {
            _restoreWindowsTimer.Stop();
            _visibleUpdaterWindows.Add(window);
            SuppressApplicationWindows();
            return;
        }

        _visibleUpdaterWindows.Remove(window);
        ScheduleRestoreApplicationWindows();
    }

    private void UpdaterWindow_Closed(object? sender, EventArgs e)
    {
        if (sender is not Window window)
        {
            return;
        }

        window.IsVisibleChanged -= UpdaterWindow_IsVisibleChanged;
        window.Closed -= UpdaterWindow_Closed;
        _visibleUpdaterWindows.Remove(window);
        ScheduleRestoreApplicationWindows();
    }

    private void SuppressApplicationWindows()
    {
        var windows = Application.Current?.Windows.Cast<Window>().ToList();
        if (windows == null)
        {
            return;
        }

        foreach (var window in windows)
        {
            if (!window.IsVisible || IsUpdaterWindow(window))
            {
                continue;
            }

            if (!_suppressedApplicationWindows.ContainsKey(window))
            {
                _suppressedApplicationWindows[window] = new WindowParkingState(
                    window.Left,
                    window.Top,
                    window.WindowStartupLocation);
            }

            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Left = -32000;
            window.Top = -32000;
        }
    }

    private void ScheduleRestoreApplicationWindows()
    {
        if (_visibleUpdaterWindows.Count > 0 || _suppressedApplicationWindows.Count == 0)
        {
            return;
        }

        _restoreWindowsTimer.Stop();
        _restoreWindowsTimer.Start();
    }

    private void RestoreApplicationWindowsIfIdle()
    {
        _restoreWindowsTimer.Stop();
        if (_visibleUpdaterWindows.Count > 0 || _suppressedApplicationWindows.Count == 0)
        {
            return;
        }

        foreach (var entry in _suppressedApplicationWindows.ToList())
        {
            if (entry.Key.IsLoaded)
            {
                entry.Key.Left = entry.Value.Left;
                entry.Key.Top = entry.Value.Top;
                entry.Key.WindowStartupLocation = entry.Value.StartupLocation;
            }
        }

        _suppressedApplicationWindows.Clear();
    }

    private static bool IsUpdaterWindow(Window window)
    {
        return window is TOTPCheckingForUpdatesWindow
            or TOTPDownloadProgressWindow
            or TOTPUpdateAvailableWindow;
    }
}
