using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TOTP.Avalonia.Desktop.Startup;

namespace TOTP.Avalonia.Desktop.Presentation;

public sealed class MainWindowViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IAvaloniaStartupCoordinator _startupCoordinator;
    private readonly AsyncCommand _initializeCommand;
    private readonly CancellationTokenSource _lifetime = new();
    private string _statusText = "Starting TOTP Manager…";
    private bool _isBusy;
    private bool _canRetry;

    public MainWindowViewModel(IAvaloniaStartupCoordinator startupCoordinator)
    {
        _startupCoordinator = startupCoordinator ?? throw new ArgumentNullException(nameof(startupCoordinator));
        _initializeCommand = new AsyncCommand(InitializeAsync, () => !_isBusy);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string StatusText
    {
        get => _statusText;
        private set => SetField(ref _statusText, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (!SetField(ref _isBusy, value)) return;
            _initializeCommand.NotifyCanExecuteChanged();
        }
    }

    public bool CanRetry
    {
        get => _canRetry;
        private set => SetField(ref _canRetry, value);
    }

    public ICommand InitializeCommand => _initializeCommand;

    public async Task InitializeAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        CanRetry = false;
        StatusText = "Starting TOTP Manager…";

        try
        {
            var outcome = await _startupCoordinator.InitializeAsync(_lifetime.Token);
            (StatusText, CanRetry) = outcome switch
            {
                AvaloniaStartupOutcome.ReadyForPasswordSetup =>
                    ("Create a master password to protect your authenticator.", false),
                AvaloniaStartupOutcome.ReadyForUnlock =>
                    ("Enter your master password to unlock your authenticator.", false),
                AvaloniaStartupOutcome.PreferencesUnavailable =>
                    ("Your preferences could not be loaded. Your encrypted data was not changed.", true),
                _ =>
                    ("TOTP Manager could not start safely. Your encrypted data was not changed.", true)
            };
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested)
        {
            StatusText = "Startup cancelled.";
        }
        catch (Exception)
        {
            StatusText = "TOTP Manager could not start safely. Your encrypted data was not changed.";
            CanRetry = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void Dispose()
    {
        _lifetime.Cancel();
        _lifetime.Dispose();
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
