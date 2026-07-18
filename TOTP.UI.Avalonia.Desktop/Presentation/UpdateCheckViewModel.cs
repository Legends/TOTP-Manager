using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TOTP.Core.Services.Interfaces;
using TOTP.Core.Services.Models;

namespace TOTP.Avalonia.Desktop.Presentation;

public sealed class UpdateCheckViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IPortableUpdateService _updates;
    private readonly IUpdateInstallerLauncher _installer;
    private readonly AsyncCommand _checkCommand;
    private readonly AsyncCommand _downloadCommand;
    private readonly AsyncCommand _installCommand;
    private readonly AsyncCommand _cancelCommand;
    private PortableUpdateOffer? _offer;
    private PortableUpdatePackage? _package;
    private CancellationTokenSource? _operationLifetime;
    private string _message = "Check the configured signed update feed when you are ready.";
    private string _version = string.Empty;
    private string _releaseNotes = string.Empty;
    private NotificationSeverity _messageSeverity = NotificationSeverity.Information;
    private int _progressPercentage;
    private bool _isProgressIndeterminate;
    private bool _isBusy;
    private bool _disposed;

    public UpdateCheckViewModel(
        IPortableUpdateService updates,
        IUpdateInstallerLauncher installer)
    {
        _updates = updates ?? throw new ArgumentNullException(nameof(updates));
        _installer = installer ?? throw new ArgumentNullException(nameof(installer));
        _checkCommand = new AsyncCommand(CheckAsync, () => !_disposed && !IsBusy);
        _downloadCommand = new AsyncCommand(
            DownloadAsync,
            () => !_disposed && !IsBusy && _offer is not null);
        _installCommand = new AsyncCommand(
            InstallAsync,
            () => !_disposed && !IsBusy && _package is not null);
        _cancelCommand = new AsyncCommand(
            CancelAsync,
            () => !_disposed && IsBusy && _operationLifetime is not null);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ICommand CheckCommand => _checkCommand;
    public ICommand DownloadCommand => _downloadCommand;
    public ICommand InstallCommand => _installCommand;
    public ICommand CancelCommand => _cancelCommand;

    public string Message
    {
        get => _message;
        private set => SetField(ref _message, value);
    }

    public string Version
    {
        get => _version;
        private set
        {
            if (!SetField(ref _version, value)) return;
            OnPropertyChanged(nameof(HasOffer));
        }
    }

    public string ReleaseNotes
    {
        get => _releaseNotes;
        private set
        {
            if (!SetField(ref _releaseNotes, value)) return;
            OnPropertyChanged(nameof(HasReleaseNotes));
        }
    }

    public NotificationSeverity MessageSeverity
    {
        get => _messageSeverity;
        private set => SetField(ref _messageSeverity, value);
    }

    public int ProgressPercentage
    {
        get => _progressPercentage;
        private set => SetField(ref _progressPercentage, Math.Clamp(value, 0, 100));
    }

    public bool IsProgressIndeterminate
    {
        get => _isProgressIndeterminate;
        private set => SetField(ref _isProgressIndeterminate, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (!SetField(ref _isBusy, value)) return;
            NotifyCommands();
        }
    }

    public bool HasOffer => Version.Length > 0;
    public bool HasReleaseNotes => ReleaseNotes.Length > 0;
    public bool IsInstallReady => _package is not null;
    public bool InstallerSupported => _installer.IsSupported;

    public async Task CheckAsync()
    {
        if (_disposed || IsBusy) return;
        ResetOffer();
        using var operation = BeginOperation();
        try
        {
            Message = "Checking the signed update feed…";
            MessageSeverity = NotificationSeverity.Information;
            var result = await _updates.CheckAsync(operation.Token);
            if (result.IsFailed)
            {
                SetFailure("The signed update feed could not be verified. No download was started.");
                return;
            }

            switch (result.Value.Status)
            {
                case PortableUpdateCheckStatus.Disabled:
                    Message = "Automatic updates are disabled or not configured for this package.";
                    MessageSeverity = NotificationSeverity.Information;
                    break;
                case PortableUpdateCheckStatus.NoUpdate:
                    Message = "No applicable signed update is available.";
                    MessageSeverity = NotificationSeverity.Success;
                    break;
                case PortableUpdateCheckStatus.UpdateAvailable when result.Value.Offer is not null:
                    _offer = result.Value.Offer;
                    Version = result.Value.Offer.Version.ToString();
                    ReleaseNotes = result.Value.Offer.ReleaseNotes;
                    Message = $"Signed update {Version} is available. Download starts only when you choose Download.";
                    MessageSeverity = NotificationSeverity.Success;
                    break;
                default:
                    SetFailure("The update response was incomplete. No download was started.");
                    break;
            }
        }
        catch (OperationCanceledException) when (operation.IsCancellationRequested)
        {
            Message = "Update check cancelled.";
            MessageSeverity = NotificationSeverity.Information;
        }
        catch (Exception)
        {
            SetFailure("The update check failed safely. No download was started.");
        }
    }

    public async Task DownloadAsync()
    {
        if (_disposed || IsBusy || _offer is null) return;
        _package = null;
        OnPropertyChanged(nameof(IsInstallReady));
        using var operation = BeginOperation();
        ProgressPercentage = 0;
        IsProgressIndeterminate = true;
        try
        {
            Message = "Downloading and verifying the signed update package…";
            MessageSeverity = NotificationSeverity.Information;
            var progress = new Progress<PortableUpdateDownloadProgress>(value =>
            {
                IsProgressIndeterminate = value.Percentage is null;
                if (value.Percentage is { } percentage) ProgressPercentage = percentage;
            });
            var result = await _updates.DownloadAsync(_offer, progress, operation.Token);
            if (result.IsFailed)
            {
                SetFailure("The update package failed download or signature verification and cannot be installed.");
                return;
            }

            _package = result.Value;
            ProgressPercentage = 100;
            IsProgressIndeterminate = false;
            OnPropertyChanged(nameof(IsInstallReady));
            _installCommand.NotifyCanExecuteChanged();
            Message = _installer.IsSupported
                ? "The signed update package is verified and ready to install."
                : "The signed update package is verified, but this desktop package has no supported installer adapter.";
            MessageSeverity = _installer.IsSupported
                ? NotificationSeverity.Success
                : NotificationSeverity.Warning;
        }
        catch (OperationCanceledException) when (operation.IsCancellationRequested)
        {
            Message = "Update download cancelled. The partial package was discarded.";
            MessageSeverity = NotificationSeverity.Information;
        }
        catch (Exception)
        {
            SetFailure("The update download failed safely. An incomplete package cannot be installed.");
        }
        finally
        {
            IsProgressIndeterminate = false;
        }
    }

    public async Task InstallAsync()
    {
        if (_disposed || IsBusy || _package is null) return;
        using var operation = BeginOperation();
        try
        {
            var result = await _installer.LaunchAsync(_package, operation.Token);
            if (result.IsFailed)
            {
                Message = "The verified update is ready, but the platform installer could not be started.";
                MessageSeverity = NotificationSeverity.Error;
                return;
            }

            Message = "The platform installer was started. Follow its prompts to complete the update.";
            MessageSeverity = NotificationSeverity.Success;
        }
        catch (OperationCanceledException) when (operation.IsCancellationRequested)
        {
            Message = "Update installation cancelled.";
            MessageSeverity = NotificationSeverity.Information;
        }
        catch (Exception)
        {
            SetFailure("The platform installer failed to start safely.");
        }
    }

    public Task CancelAsync()
    {
        _operationLifetime?.Cancel();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _operationLifetime?.Cancel();
        _operationLifetime?.Dispose();
        _operationLifetime = null;
        ResetOffer();
        NotifyCommands();
    }

    private OperationLease BeginOperation()
    {
        _operationLifetime?.Dispose();
        _operationLifetime = new CancellationTokenSource();
        IsBusy = true;
        return new OperationLease(this, _operationLifetime);
    }

    private void EndOperation(CancellationTokenSource lifetime)
    {
        if (!ReferenceEquals(_operationLifetime, lifetime)) return;
        _operationLifetime = null;
        lifetime.Dispose();
        IsBusy = false;
    }

    private void ResetOffer()
    {
        _offer = null;
        _package = null;
        Version = string.Empty;
        ReleaseNotes = string.Empty;
        ProgressPercentage = 0;
        IsProgressIndeterminate = false;
        OnPropertyChanged(nameof(IsInstallReady));
        NotifyCommands();
    }

    private void SetFailure(string message)
    {
        Message = message;
        MessageSeverity = NotificationSeverity.Error;
    }

    private void NotifyCommands()
    {
        _checkCommand.NotifyCanExecuteChanged();
        _downloadCommand.NotifyCanExecuteChanged();
        _installCommand.NotifyCanExecuteChanged();
        _cancelCommand.NotifyCanExecuteChanged();
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private sealed class OperationLease(
        UpdateCheckViewModel owner,
        CancellationTokenSource lifetime) : IDisposable
    {
        private bool _disposed;
        public CancellationToken Token => lifetime.Token;
        public bool IsCancellationRequested => lifetime.IsCancellationRequested;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            owner.EndOperation(lifetime);
        }
    }
}
