using Microsoft.Extensions.Logging;
using NetSparkleUpdater;
using NetSparkleUpdater.Interfaces;
using NetSparkleUpdater.UI.WPF;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace TOTP.AutoUpdate;

internal sealed class TOTPNetSparkleUiFactory : IUIFactory
{
    private readonly UIFactory _innerFactory = new();
    private readonly Func<AppCastItem, string?, Task<bool>>? _customInstallHandler;
    private readonly ILogger<AutoUpdateDialogWindow>? _progressWindowLogger;
    private AutoUpdateDialogWindow? _dialogWindow;
    private UnifiedDownloadProgress? _activeProgress;

    public TOTPNetSparkleUiFactory(
        Func<AppCastItem, string?, Task<bool>>? customInstallHandler = null,
        ILogger<AutoUpdateDialogWindow>? progressWindowLogger = null)
    {
        _customInstallHandler = customInstallHandler;
        _progressWindowLogger = progressWindowLogger;
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
        return InvokeOnUi(() => new UnifiedUpdateAvailable(GetOrCreateDialogWindow(), updates, isUpdateAlreadyDownloaded));
    }

    public IDownloadProgress CreateProgressWindow(SparkleUpdater sparkle, AppCastItem item)
    {
        return InvokeOnUi(() =>
        {
            _activeProgress = new UnifiedDownloadProgress(GetOrCreateDialogWindow(), item, _customInstallHandler, _progressWindowLogger);
            return _activeProgress;
        });
    }

    public ICheckingForUpdates ShowCheckingForUpdates(SparkleUpdater sparkle)
    {
        return InvokeOnUi(() => new UnifiedCheckingForUpdates(GetOrCreateDialogWindow()));
    }

    public void Init(SparkleUpdater sparkle)
    {
        _innerFactory.Init(sparkle);
    }

    public void ShowUnknownInstallerFormatMessage(SparkleUpdater sparkle, string downloadFileName)
    {
        RestoreParkedWindows();
        _innerFactory.ShowUnknownInstallerFormatMessage(sparkle, downloadFileName);
    }

    public void ShowVersionIsUpToDate(SparkleUpdater sparkle)
    {
        RestoreParkedWindows();
        _innerFactory.ShowVersionIsUpToDate(sparkle);
    }

    public void ShowVersionIsSkippedByUserRequest(SparkleUpdater sparkle)
    {
        RestoreParkedWindows();
        _innerFactory.ShowVersionIsSkippedByUserRequest(sparkle);
    }

    public void ShowCannotDownloadAppcast(SparkleUpdater sparkle, string appcastUrl)
    {
        RestoreParkedWindows();
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
        if (_activeProgress?.DisplayErrorMessage(message) == true)
        {
            return;
        }

        RestoreParkedWindows();
        _innerFactory.ShowDownloadErrorMessage(sparkle, message, appcastUrl);
    }

    public void Shutdown(SparkleUpdater sparkle)
    {
        _innerFactory.Shutdown(sparkle);
    }

    public void SetDownloadedFilePath(AppCastItem item, string? downloadedFilePath)
    {
        _activeProgress?.SetDownloadedFilePath(downloadedFilePath);
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

    private static void InvokeOnUi(Action action)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher == null || dispatcher.CheckAccess())
        {
            action();
            return;
        }

        dispatcher.Invoke(action);
    }

    private AutoUpdateDialogWindow GetOrCreateDialogWindow()
    {
        _dialogWindow ??= new AutoUpdateDialogWindow();
        return _dialogWindow;
    }

    private void RestoreParkedWindows()
    {
        InvokeOnUi(() => _dialogWindow?.RestoreBackgroundWindowsNow());
    }
}
