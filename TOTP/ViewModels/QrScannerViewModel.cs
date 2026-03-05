using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.Logging;
using TOTP.Commands;
using TOTP.Services.Interfaces;

namespace TOTP.ViewModels
{
    public sealed class QrScannerViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly IQrScannerRunner _qrScannerRunner;
        private readonly ILogger<QrScannerViewModel> _logger;
        private CancellationTokenSource? _cts;
        private Task? _cameraTask;

        private BitmapSource? _previewImage;
        public BitmapSource? PreviewImage
        {
            get => _previewImage;
            private set { _previewImage = value; OnPropertyChanged(); }
        }

        private bool _isInitializing = true;
        public bool IsInitializing
        {
            get => _isInitializing;
            private set { _isInitializing = value; OnPropertyChanged(); }
        }

        private string? _decodedText;
        public string? DecodedText
        {
            get => _decodedText;
            private set { _decodedText = value; OnPropertyChanged(); }
        }

        private string? _errorMessage;
        public string? ErrorMessage
        {
            get => _errorMessage;
            private set { _errorMessage = value; OnPropertyChanged(); }
        }

        // Window will subscribe and close itself (and set DialogResult)
        public event EventHandler<CloseRequestedEventArgs>? CloseRequested;

        public ICommand CancelCommand { get; }
        public ICommand CloseCameraWindowCommand { get; } // for Escape binding

        public QrScannerViewModel(IQrScannerRunner qrScannerRunner, ILogger<QrScannerViewModel> logger)
        {
            _qrScannerRunner = qrScannerRunner;
            _logger = logger;
            CancelCommand = new RelayCommand(Cancel, CanCancelOrClose);
            CloseCameraWindowCommand = new RelayCommand(Cancel, CanCancelOrClose);
        }

        public void Start()
        {
            if (_cameraTask != null) return;

            IsInitializing = true;
            ErrorMessage = null;

            _cts = new CancellationTokenSource();
            _cameraTask = RunCameraLoopAsync(_cts.Token);
            RaiseCommandStates();
        }

        public void Stop()
        {
            _cts?.Cancel();
        }

        private bool CanCancelOrClose()
        {
            return _cameraTask == null || !_cameraTask.IsCompleted;
        }

        private void Cancel()
        {
            Stop();
            RequestClose(dialogResult: false);
        }

        private void RequestClose(bool dialogResult)
        {
            CloseRequested?.Invoke(this, new CloseRequestedEventArgs(dialogResult));
        }

        private async Task RunCameraLoopAsync(CancellationToken token)
        {
            try
            {
                await _qrScannerRunner.RunAsync(
                    token,
                    onPreview: bmp => PreviewImage = bmp,
                    onFirstFrame: () => IsInitializing = false,
                    onDecoded: decoded =>
                    {
                        DecodedText = decoded;
                        RequestClose(dialogResult: true);
                    });
            }
            catch (OperationCanceledException)
            {
                // expected on cancel
                _logger.LogDebug("QR scanner camera loop cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "QR scanner camera loop failed.");
                ErrorMessage = ex.Message;
                RequestClose(dialogResult: false);
            }
            finally
            {
                RaiseCommandStates();
            }
        }

        public void Dispose()
        {
            try
            {
                _cts?.Cancel();
                _cts?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to dispose QR scanner cancellation token source.");
            }
            finally
            {
                RaiseCommandStates();
            }
        }

        private void RaiseCommandStates()
        {
            if (CancelCommand is RelayCommand cancelCommand)
            {
                cancelCommand.RaiseCanExecuteChanged();
            }

            if (CloseCameraWindowCommand is RelayCommand closeCommand)
            {
                closeCommand.RaiseCanExecuteChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public sealed class CloseRequestedEventArgs : EventArgs
    {
        public bool DialogResult { get; }
        public CloseRequestedEventArgs(bool dialogResult) => DialogResult = dialogResult;
    }
}
