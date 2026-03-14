using NetSparkleUpdater;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TOTP.Resources;

namespace TOTP.AutoUpdate;

public sealed partial class AutoUpdateDialogWindow : AutoUpdateWindowBase
{
    private static readonly TimeSpan MinimumReadyDelay = TimeSpan.FromMilliseconds(900);
    private static readonly TimeSpan BackgroundRestoreDelay = TimeSpan.FromMilliseconds(450);
    private readonly AutoUpdateDialogState _state = new();
    private readonly ApplicationWindowParking _windowParking = new();
    private AppCastItem? _currentProgressItem;
    private DateTimeOffset _progressStartedAtUtc;
    private bool _downloadFinished;
    private bool _downloadedFileValid;
    private bool _readyStateApplied;
    private bool _terminalErrorDisplayed;
    private int _lastProgressPercentage;
    private DispatcherTimer? _restoreTimer;

    public AutoUpdateDialogWindow()
    {
        InitializeComponent();
        DataContext = _state;
        Closing += AutoUpdateDialogWindow_Closing;
        SizeChanged += (_, _) =>
        {
            if (IsVisible)
            {
                RecenterOwnedWindow();
            }
        };
        _state.UpdateResponseRequested += OnUpdateResponseRequested;
        _state.ProgressActionRequested += OnProgressActionRequested;
        _state.PropertyChanged += State_PropertyChanged;
        Loaded += (_, _) =>
        {
            UpdateVisualState(useTransitions: false);
            RecenterOwnedWindow();
        };
    }

    internal bool SuppressPresentation { get; set; }

    public Action<UpdateAvailableResult, AppCastItem>? UpdateResponseHandler { get; set; }

    public Func<Task>? ProgressActionHandler { get; set; }

    internal AutoUpdateDialogState State => _state;

    public void ShowChecking()
    {
        InvokeOnUi(() =>
        {
            _state.ShowChecking();
            EnsurePresented(parkApplicationWindows: false);
        });
    }

    public void ShowUpdateAvailable(
        List<AppCastItem> updates,
        bool isUpdateAlreadyDownloaded,
        bool hideReleaseNotes,
        bool hideRemindMeLaterButton,
        bool hideSkipButton)
    {
        ArgumentNullException.ThrowIfNull(updates);
        if (updates.Count == 0)
        {
            throw new ArgumentException("At least one update candidate is required.", nameof(updates));
        }

        InvokeOnUi(() =>
        {
            _state.ShowAvailable(
                updates,
                isUpdateAlreadyDownloaded,
                hideReleaseNotes,
                hideRemindMeLaterButton,
                hideSkipButton);
            EnsurePresented(parkApplicationWindows: true);
        });
    }

    public void ShowDownloadProgress(AppCastItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        InvokeOnUi(() =>
        {
            _currentProgressItem = item;
            _progressStartedAtUtc = DateTimeOffset.UtcNow;
            _downloadFinished = false;
            _downloadedFileValid = false;
            _readyStateApplied = false;
            _terminalErrorDisplayed = false;
            _lastProgressPercentage = 0;
            _state.ShowProgress(item);
            EnsurePresented(parkApplicationWindows: true);
        });
    }

    public void BringToFront()
    {
        InvokeOnUi(() =>
        {
            if (!SuppressPresentation && !IsVisible)
            {
                ShowOwnedWindow();
            }

            if (!SuppressPresentation)
            {
                Activate();
                Topmost = true;
                Topmost = false;
                Focus();
            }
        });
    }

    public void CloseDialog()
    {
        InvokeOnUi(() =>
        {
            UpdateResponseHandler = null;
            ProgressActionHandler = null;
            Hide();
            ScheduleBackgroundRestore();
        });
    }

    public void CloseCheckingIfVisible()
    {
        InvokeOnUi(() =>
        {
            if (_state.CurrentStep != AutoUpdateDialogStep.Checking)
            {
                return;
            }

            CloseDialog();
        });
    }

    public void RestoreBackgroundWindowsNow()
    {
        InvokeOnUi(() =>
        {
            CancelBackgroundRestore();
            _windowParking.Restore();
        });
    }

    public void SetDownloadAndInstallButtonEnabled(bool shouldBeEnabled)
    {
        if ((_readyStateApplied || _terminalErrorDisplayed) && !shouldBeEnabled)
        {
            return;
        }

        InvokeOnUi(() => _state.SetProgressActionEnabled(shouldBeEnabled));
    }

    public void ShowInstallingState()
    {
        InvokeOnUi(() => _state.ShowInstallingState());
    }

    public void ShowHelperFailedState()
    {
        InvokeOnUi(() => _state.ShowHelperFailedState());
    }

    public void SetDownloadedFilePath(string? downloadedFilePath)
    {
    }

    public void OnDownloadProgressChanged(ItemDownloadProgressEventArgs args)
    {
        if (_terminalErrorDisplayed)
        {
            return;
        }

        _lastProgressPercentage = Math.Clamp(args.ProgressPercentage, 0, 100);
        InvokeOnUi(() =>
        {
            _state.SetProgress(
                _lastProgressPercentage,
                string.Format(
                    UI.ui_Updater_Download_Progress_Format,
                    args.ProgressPercentage,
                    FormatBytes(args.BytesReceived),
                    FormatBytes(args.TotalBytesToReceive)));
        });

        if (_downloadFinished)
        {
            _ = TryApplyFinishedStateAsync();
        }
    }

    public void FinishedDownloadingFile(bool isDownloadedFileValid)
    {
        if (_terminalErrorDisplayed)
        {
            return;
        }

        _downloadFinished = true;
        _downloadedFileValid = isDownloadedFileValid;
        if (isDownloadedFileValid)
        {
            _lastProgressPercentage = 100;
        }

        _ = TryApplyFinishedStateAsync();
    }

    public bool DisplayErrorMessage(string errorMessage)
    {
        _terminalErrorDisplayed = true;
        _downloadFinished = false;
        _downloadedFileValid = false;
        _readyStateApplied = true;

        InvokeOnUi(() =>
        {
            _state.SetProgressError(errorMessage, Math.Clamp(_lastProgressPercentage, 0, 100));
            EnsurePresented(parkApplicationWindows: true);
        });

        return true;
    }

    private async Task TryApplyFinishedStateAsync()
    {
        if (_readyStateApplied || !_downloadFinished)
        {
            return;
        }

        var remainingDelay = MinimumReadyDelay - (DateTimeOffset.UtcNow - _progressStartedAtUtc);
        if (remainingDelay > TimeSpan.Zero)
        {
            await Task.Delay(remainingDelay);
        }

        InvokeOnUi(() =>
        {
            if (_readyStateApplied)
            {
                return;
            }

            _readyStateApplied = true;
            if (_downloadedFileValid)
            {
                _state.SetProgressReady();
                return;
            }

            _state.SetProgressBlocked();
        });
    }

    private void EnsurePresented(bool parkApplicationWindows)
    {
        if (SuppressPresentation)
        {
            return;
        }

        CancelBackgroundRestore();
        CaptureCenterAnchor(ResolveCenterAnchorWindow());
        if (parkApplicationWindows)
        {
            _windowParking.Park(this);
        }

        ShowOwnedWindow();
    }

    protected override void OnClosed(EventArgs e)
    {
        CancelBackgroundRestore();
        ClearCenterAnchor();
        _windowParking.Restore();
        Closing -= AutoUpdateDialogWindow_Closing;
        _state.PropertyChanged -= State_PropertyChanged;
        base.OnClosed(e);
    }

    private void OnUpdateResponseRequested(UpdateAvailableResult result, AppCastItem item)
    {
        UpdateResponseHandler?.Invoke(result, item);

        if (result == UpdateAvailableResult.InstallUpdate)
        {
            if (_currentProgressItem == null)
            {
                _currentProgressItem = item;
            }

            _state.ShowProgress(item);
            return;
        }

        CloseDialog();
    }

    private async void OnProgressActionRequested()
    {
        var progressActionHandler = ProgressActionHandler;
        if (progressActionHandler == null)
        {
            CloseDialog();
            return;
        }

        await progressActionHandler();
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes <= 0)
        {
            return "0 B";
        }

        string[] sizes = ["B", "KB", "MB", "GB"];
        var value = (double)bytes;
        var order = 0;
        while (value >= 1024 && order < sizes.Length - 1)
        {
            order++;
            value /= 1024;
        }

        return $"{value:0.#} {sizes[order]}";
    }

    private void ScheduleBackgroundRestore()
    {
        if (SuppressPresentation)
        {
            return;
        }

        CancelBackgroundRestore();
        _restoreTimer = new DispatcherTimer(
            BackgroundRestoreDelay,
            DispatcherPriority.Normal,
            RestoreBackgroundWindowsIfStillHidden,
            Dispatcher);
        _restoreTimer.Start();
    }

    private void CancelBackgroundRestore()
    {
        if (_restoreTimer == null)
        {
            return;
        }

        _restoreTimer.Stop();
        _restoreTimer.Tick -= RestoreBackgroundWindowsIfStillHidden;
        _restoreTimer = null;
    }

    private void RestoreBackgroundWindowsIfStillHidden(object? sender, EventArgs e)
    {
        CancelBackgroundRestore();
        if (!IsVisible)
        {
            _windowParking.Restore();
        }
    }

    private void AutoUpdateDialogWindow_Closing(object? sender, CancelEventArgs e)
    {
        e.Cancel = true;

        HandleSystemCloseRequest();
    }

    private void HandleSystemCloseRequest()
    {
        switch (_state.CurrentStep)
        {
            case AutoUpdateDialogStep.Checking:
                CloseDialog();
                break;

            case AutoUpdateDialogStep.Available:
                var offer = _state.SelectedUpdate;
                if (offer != null)
                {
                    OnUpdateResponseRequested(UpdateAvailableResult.RemindMeLater, offer.Item);
                    break;
                }

                CloseDialog();
                break;

            case AutoUpdateDialogStep.Progress:
                CloseDialog();
                break;

            default:
                RestoreBackgroundWindowsNow();
                Hide();
                break;
        }
    }

    private void State_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AutoUpdateDialogState.CurrentStep))
        {
            InvokeOnUi(() =>
            {
                UpdateVisualState(useTransitions: IsVisible);
                Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(RecenterOwnedWindow));
            });
        }
    }

    private void UpdateVisualState(bool useTransitions)
    {
        var stateName = _state.CurrentStep switch
        {
            AutoUpdateDialogStep.Available => "Available",
            AutoUpdateDialogStep.Progress => "Progress",
            _ => "Checking"
        };

        VisualStateManager.GoToElementState(DialogRoot, stateName, useTransitions);
    }

    private Window? ResolveCenterAnchorWindow()
    {
        var application = Application.Current;
        if (application == null)
        {
            return Owner ?? null;
        }

        return application.Windows
            .OfType<Window>()
            .Where(window => window.IsVisible && !ReferenceEquals(window, this) && window is not AutoUpdateDialogWindow)
            .OrderByDescending(window => window.IsActive)
            .ThenByDescending(window => window == Owner)
            .ThenByDescending(window => window == application.MainWindow)
            .FirstOrDefault();
    }
}
