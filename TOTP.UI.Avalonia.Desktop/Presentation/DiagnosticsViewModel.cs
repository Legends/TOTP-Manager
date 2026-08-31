using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using TOTP.Core.Services.Interfaces;
using TOTP.Avalonia.Desktop.Platform;
using TOTP.Avalonia.Desktop.Presentation.Dialogs;

namespace TOTP.Avalonia.Desktop.Presentation;

public sealed class DiagnosticsViewModel : INotifyPropertyChanged
{
    private readonly ISupportDiagnosticsService _diagnostics;
    private readonly IAvaloniaDialogService? _dialogs;
    private readonly IPlatformCapabilityReport? _capabilities;
    private readonly AsyncCommand _refreshCommand;
    private string _supportInformation = string.Empty;
    private bool _isBusy;

    public DiagnosticsViewModel(
        ISupportDiagnosticsService diagnostics,
        IAvaloniaDialogService? dialogs = null,
        IPlatformCapabilityReport? capabilities = null)
    {
        _diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
        _dialogs = dialogs;
        _capabilities = capabilities;
        Notification = new NotificationState();
        _refreshCommand = new AsyncCommand(RefreshAsync, () => !_isBusy);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string SupportInformation
    {
        get => _supportInformation;
        private set => SetField(ref _supportInformation, value);
    }

    public NotificationState Notification { get; }
    public string Message => Notification.Text;
    public NotificationSeverity MessageSeverity => Notification.Severity;
    public bool HasMessage => Notification.HasMessage;
    public ICommand RefreshCommand => _refreshCommand;

    public async Task RefreshAsync()
    {
        if (_isBusy) return;
        _isBusy = true;
        _refreshCommand.NotifyCanExecuteChanged();
        try
        {
            var snapshot = _diagnostics.Capture();
            var output = new StringBuilder()
                .AppendLine($"TOTP Manager {snapshot.ApplicationVersion}")
                .AppendLine($"Platform: {snapshot.OperatingSystem}")
                .AppendLine($"Architecture: {snapshot.ProcessArchitecture}")
                .AppendLine($"Runtime: {snapshot.Framework}")
                .AppendLine($"Log directory configured: {(snapshot.LogDirectoryConfigured ? "yes" : "no")}");
            if (snapshot.StartupRecords.Count > 0)
            {
                output.AppendLine("Startup stages:");
                foreach (var record in snapshot.StartupRecords)
                {
                    output.AppendLine(
                        $"- {record.Stage}: {record.ElapsedMilliseconds} ms ({(record.Succeeded ? "ok" : "failed")})");
                }
            }

            if (_capabilities is not null)
            {
                var capabilities = await _capabilities.CaptureAsync();
                output.AppendLine("Platform capabilities:");
                foreach (var capability in capabilities)
                    output.AppendLine($"- {capability.Name}: {capability.Status}");
            }

            SupportInformation = output.ToString().TrimEnd();
            Notification.ShowPersistent(
                "Support information refreshed. It contains no account data or filesystem paths.",
                NotificationSeverity.Success);
        }
        catch (Exception)
        {
            SupportInformation = string.Empty;
            Notification.ShowPersistent(
                "Support information could not be collected safely.",
                NotificationSeverity.Error);
            if (_dialogs is not null)
            {
                try
                {
                    await _dialogs.ShowMessageAsync(new MessageDialogRequest(
                        "Support diagnostics unavailable",
                        "Support information could not be collected. You can close this message and continue using the application.",
                        NotificationSeverity.Error,
                        "Close"));
                }
                catch (Exception)
                {
                    // The non-modal banner remains the safe recovery path.
                }
            }
        }
        finally
        {
            _isBusy = false;
            _refreshCommand.NotifyCanExecuteChanged();
        }

    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
