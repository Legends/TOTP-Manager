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
    private readonly ILogger<TOTPDownloadProgressWindow>? _progressWindowLogger;
    private TOTPDownloadProgressWindow? _activeProgressWindow;

    public TOTPNetSparkleUiFactory(
        Func<AppCastItem, string?, Task<bool>>? customInstallHandler = null,
        ILogger<TOTPDownloadProgressWindow>? progressWindowLogger = null)
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
        return InvokeOnUi(() => new TOTPUpdateAvailableWindow(updates, isUpdateAlreadyDownloaded));
    }

    public IDownloadProgress CreateProgressWindow(SparkleUpdater sparkle, AppCastItem item)
    {
        return InvokeOnUi(() =>
        {
            var window = new TOTPDownloadProgressWindow(item, _customInstallHandler, _progressWindowLogger);
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
        return InvokeOnUi(() => new TOTPCheckingForUpdatesWindow());
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
}
