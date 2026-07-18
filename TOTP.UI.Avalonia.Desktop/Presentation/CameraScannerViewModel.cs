using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Windows.Input;
using Avalonia.Media;
using Microsoft.Extensions.Logging;
using TOTP.Avalonia.Desktop.Platform;
using TOTP.Core.Services.Interfaces;

namespace TOTP.Avalonia.Desktop.Presentation;

public sealed class CameraScannerViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IQrScannerRunner _runner;
    private readonly IQrPayloadValidator _payloadValidator;
    private readonly IAvaloniaQrImageFactory _imageFactory;
    private readonly IUiScheduler _uiScheduler;
    private readonly ILogger<CameraScannerViewModel> _logger;
    private readonly AsyncCommand _startCommand;
    private readonly AsyncCommand _cancelCommand;
    private CancellationTokenSource? _captureLifetime;
    private AvaloniaQrImageHandle? _preview;
    private string _message = "Camera access starts only when you choose Scan QR.";
    private bool _isScanning;
    private bool _disposed;
    private long _generation;

    public CameraScannerViewModel(
        IQrScannerRunner runner,
        IQrPayloadValidator payloadValidator,
        IAvaloniaQrImageFactory imageFactory,
        IUiScheduler uiScheduler,
        ILogger<CameraScannerViewModel> logger)
    {
        _runner = runner ?? throw new ArgumentNullException(nameof(runner));
        _payloadValidator = payloadValidator ?? throw new ArgumentNullException(nameof(payloadValidator));
        _imageFactory = imageFactory ?? throw new ArgumentNullException(nameof(imageFactory));
        _uiScheduler = uiScheduler ?? throw new ArgumentNullException(nameof(uiScheduler));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _startCommand = new AsyncCommand(StartAsync, () => !_disposed && !IsScanning);
        _cancelCommand = new AsyncCommand(CancelAsync, () => !_disposed && IsScanning);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ICommand StartCommand => _startCommand;

    public ICommand CancelCommand => _cancelCommand;

    public string Message
    {
        get => _message;
        private set => SetField(ref _message, value);
    }

    public bool IsScanning
    {
        get => _isScanning;
        private set
        {
            if (!SetField(ref _isScanning, value)) return;
            _startCommand.NotifyCanExecuteChanged();
            _cancelCommand.NotifyCanExecuteChanged();
        }
    }

    public IImage? PreviewImage => _preview?.Image;

    public bool HasPreview => PreviewImage is not null;

    public async Task StartAsync()
    {
        if (_disposed || IsScanning) return;

        ClearPreview();
        var generation = Interlocked.Increment(ref _generation);
        _captureLifetime?.Dispose();
        _captureLifetime = new CancellationTokenSource();
        var token = _captureLifetime.Token;
        IsScanning = true;
        Message = "Requesting camera access…";

        try
        {
            var result = await _runner.RunAsync(
                token,
                bytes => QueuePreview(bytes, generation),
                () => QueueUi(generation, () => Message = "Camera active. Point it at a TOTP QR code."));

            if (generation != Volatile.Read(ref _generation) || _disposed) return;

            if (result.IsDecoded)
            {
                var validation = _payloadValidator.Validate(result.DecodedText!);
                Message = validation.IsValid
                    ? BuildValidatedMessage(validation)
                    : "The QR code is not a valid supported TOTP account.";
            }
            else
            {
                _logger.LogWarning("Avalonia camera scan stopped. failure={Failure}", result.Failure);
                Message = FailureMessage(result.Failure);
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            if (!_disposed && generation == Volatile.Read(ref _generation))
                Message = "QR scan cancelled.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Avalonia camera scan failed at the platform boundary.");
            if (!_disposed && generation == Volatile.Read(ref _generation))
                Message = "The camera scanner failed safely. No account data was changed.";
        }
        finally
        {
            if (!_disposed && generation == Volatile.Read(ref _generation))
                IsScanning = false;
        }
    }

    public Task CancelAsync()
    {
        _captureLifetime?.Cancel();
        return Task.CompletedTask;
    }

    public void Clear()
    {
        Interlocked.Increment(ref _generation);
        _captureLifetime?.Cancel();
        _captureLifetime?.Dispose();
        _captureLifetime = null;
        IsScanning = false;
        Message = "Camera access starts only when you choose Scan QR.";
        ClearPreview();
    }

    public void Dispose()
    {
        if (_disposed) return;
        Clear();
        _disposed = true;
        _startCommand.NotifyCanExecuteChanged();
        _cancelCommand.NotifyCanExecuteChanged();
    }

    private void QueuePreview(byte[] encodedFrame, long generation)
    {
        if (_disposed || generation != Volatile.Read(ref _generation))
        {
            CryptographicOperations.ZeroMemory(encodedFrame);
            return;
        }

        try
        {
            _uiScheduler.Post(() =>
            {
                try
                {
                    if (_disposed || generation != Volatile.Read(ref _generation)) return;
                    var replacement = _imageFactory.Create(encodedFrame);
                    var previous = _preview;
                    _preview = replacement;
                    OnPropertyChanged(nameof(PreviewImage));
                    OnPropertyChanged(nameof(HasPreview));
                    previous?.Dispose();
                }
                finally
                {
                    CryptographicOperations.ZeroMemory(encodedFrame);
                }
            });
        }
        catch
        {
            CryptographicOperations.ZeroMemory(encodedFrame);
            throw;
        }
    }

    private void QueueUi(long generation, Action action)
    {
        _uiScheduler.Post(() =>
        {
            if (!_disposed && generation == Volatile.Read(ref _generation))
                action();
        });
    }

    private void ClearPreview()
    {
        var preview = _preview;
        _preview = null;
        OnPropertyChanged(nameof(PreviewImage));
        OnPropertyChanged(nameof(HasPreview));
        preview?.Dispose();
    }

    private static string BuildValidatedMessage(QrPayloadValidationResult validation)
    {
        var displayName = string.Join(
            " / ",
            new[] { validation.Issuer, validation.AccountName }.Where(value => value.Length > 0));
        return displayName.Length == 0
            ? "A valid TOTP QR code was scanned. Import remains disabled in this technical preview."
            : $"Validated TOTP account: {displayName}. Import remains disabled in this technical preview.";
    }

    private static string FailureMessage(QrScannerFailureKind failure) => failure switch
    {
        QrScannerFailureKind.PermissionDenied => "Camera permission was denied. No account data was changed.",
        QrScannerFailureKind.NativeRuntimeUnavailable => "The camera runtime is unavailable for this package.",
        QrScannerFailureKind.DeviceLost => "The camera was disconnected or stopped responding.",
        QrScannerFailureKind.Stalled => "The camera stopped providing new frames.",
        QrScannerFailureKind.NoCamera => "No available camera was found.",
        _ => "The camera scanner could not start safely."
    };

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
