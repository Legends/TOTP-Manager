using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using TOTP.Commands;
using TOTP.Core.Services.Interfaces;
using TOTP.Resources;
using TOTP.Services.Interfaces;

namespace TOTP.ViewModels
{
    public sealed class QrScannerViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly IQrScannerRunner _qrScannerRunner;
        private readonly ILogger<QrScannerViewModel> _logger;
        private readonly IUiScheduler? _dispatcher;
        private CancellationTokenSource? _cts;
        private Task? _cameraTask;
        private bool _closeRequested;
        private bool _disposed;

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
            private set
            {
                _errorMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasError));
            }
        }
        public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

        // Window will subscribe and close itself (and set DialogResult)
        public event EventHandler<CloseRequestedEventArgs>? CloseRequested;

        public ICommand CancelCommand { get; }
        public ICommand CloseCameraWindowCommand { get; } // for Escape binding

        public QrScannerViewModel(
            IQrScannerRunner qrScannerRunner,
            ILogger<QrScannerViewModel> logger,
            IUiScheduler? dispatcher = null)
        {
            _qrScannerRunner = qrScannerRunner;
            _logger = logger;
            _dispatcher = dispatcher;
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
            return !_closeRequested;
        }

        private void Cancel()
        {
            Stop();
            RequestClose(dialogResult: false);
        }

        private void RequestClose(bool dialogResult)
        {
            if (_closeRequested)
            {
                return;
            }

            _closeRequested = true;
            RaiseCommandStates();
            CloseRequested?.Invoke(this, new CloseRequestedEventArgs(dialogResult));
        }

        private async Task RunCameraLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await _qrScannerRunner.RunAsync(
                        token,
                        onPreview: UpdatePreview,
                        onFirstFrame: () => RunOnUi(() =>
                        {
                            ErrorMessage = null;
                            IsInitializing = false;
                        }),
                        onDecoded: decoded => RunOnUi(() =>
                        {
                            DecodedText = decoded;
                            RequestClose(dialogResult: true);
                        }));
                    return;
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    _logger.LogDebug("QR scanner camera loop cancelled.");
                    return;
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogWarning(ex, "QR scanner camera is unavailable; retrying.");
                    RunOnUi(() =>
                    {
                        ErrorMessage = UI.ResourceManager.GetString("ui_QrScanner_CameraUnavailable");
                        IsInitializing = false;
                    });

                    try
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(500), token);
                    }
                    catch (OperationCanceledException) when (token.IsCancellationRequested)
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "QR scanner camera loop failed.");
                    RunOnUi(() =>
                    {
                        ErrorMessage = UI.ResourceManager.GetString("ui_QrScanner_UnexpectedError");
                        IsInitializing = false;
                    });
                    return;
                }
                finally
                {
                    RunOnUi(RaiseCommandStates);
                }
            }
        }

        private void UpdatePreview(byte[] encodedFrame)
        {
            void DecodeAndClear()
            {
                try
                {
                    if (!_disposed)
                    {
                        PreviewImage = DecodePreview(encodedFrame);
                    }
                }
                finally
                {
                    CryptographicOperations.ZeroMemory(encodedFrame);
                }
            }

            if (_disposed)
            {
                CryptographicOperations.ZeroMemory(encodedFrame);
            }
            else if (_dispatcher == null || _dispatcher.CheckAccess())
            {
                DecodeAndClear();
            }
            else
            {
                try
                {
                    _dispatcher.Post(DecodeAndClear);
                }
                catch
                {
                    CryptographicOperations.ZeroMemory(encodedFrame);
                    throw;
                }
            }
        }

        private static BitmapSource DecodePreview(byte[] encodedFrame)
        {
            using var stream = new MemoryStream(encodedFrame, writable: false);
            var image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = stream;
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            image.Freeze();
            return image;
        }

        public void Dispose()
        {
            _disposed = true;
            try
            {
                var cts = Interlocked.Exchange(ref _cts, null);
                if (cts != null)
                {
                    try
                    {
                        cts.Cancel();
                    }
                    catch (ObjectDisposedException)
                    {
                        // Already disposed by another shutdown path.
                    }

                    cts.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to dispose QR scanner cancellation token source.");
            }

            RaiseCommandStates();
        }

        private void RunOnUi(Action action)
        {
            if (_disposed)
            {
                return;
            }

            if (_dispatcher == null || _dispatcher.CheckAccess())
            {
                action();
                return;
            }

            _dispatcher.Post(() =>
            {
                if (!_disposed)
                {
                    action();
                }
            });
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
