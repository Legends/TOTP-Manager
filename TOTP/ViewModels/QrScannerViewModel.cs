using OpenCvSharp;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TOTP.Commands;
using TOTP.Infrastructure.Extensions;

namespace TOTP.ViewModels
{
    public sealed class QrScannerViewModel : INotifyPropertyChanged, IDisposable
    {
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

        public QrScannerViewModel()
        {
            CancelCommand = new RelayCommand(Cancel);
            CloseCameraWindowCommand = new RelayCommand(Cancel);
        }

        public void Start()
        {
            if (_cameraTask != null) return;

            IsInitializing = true;
            ErrorMessage = null;

            _cts = new CancellationTokenSource();
            _cameraTask = RunCameraLoopAsync(_cts.Token);
        }

        public void Stop()
        {
            _cts?.Cancel();
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
            VideoCapture? cap = null;

            try
            {
                // Open camera on background thread
                cap = await Task.Run(() =>
                {
                    try
                    {
                        var c = new VideoCapture(0, VideoCaptureAPIs.DSHOW);
                        if (!c.IsOpened()) c.Open(0);
                        if (!c.IsOpened()) return null;

                        c.Set(VideoCaptureProperties.FrameWidth, 1280);
                        c.Set(VideoCaptureProperties.FrameHeight, 720);
                        c.Set(VideoCaptureProperties.Fps, 30);
                        try { c.Set(VideoCaptureProperties.FourCC, FourCC.MJPG); } catch { }
                        try { c.Set(VideoCaptureProperties.BufferSize, 1); } catch { }

                        return c;
                    }
                    catch
                    {
                        return null;
                    }
                }, token).ConfigureAwait(false);

                if (cap == null)
                {
                    ErrorMessage = "No camera found.";
                    RequestClose(dialogResult: false);
                    return;
                }

                using var frame = new Mat();
                var detector = new QRCodeDetector();
                var firstFrameShown = false;

                while (!token.IsCancellationRequested)
                {
                    if (!cap.Read(frame) || frame.Empty())
                        continue;

                    // Decode
                    var decoded = detector.DetectAndDecode(frame, out _);
                    if (!string.IsNullOrEmpty(decoded))
                    {
                        DecodedText = decoded;
                        RequestClose(dialogResult: true);
                        return;
                    }

                    // Preview: create BitmapSource off UI-thread, Freeze it, assign (safe)
                    var bmp = frame.ToBitmapSource();
                    bmp.Freeze();

                    PreviewImage = bmp;

                    if (!firstFrameShown)
                    {
                        IsInitializing = false;
                        firstFrameShown = true;
                    }

                    await Task.Delay(10, token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // expected on cancel
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                RequestClose(dialogResult: false);
            }
            finally
            {
                try { cap?.Release(); } catch { }
                cap?.Dispose();
            }
        }

        public void Dispose()
        {
            try
            {
                _cts?.Cancel();
                _cts?.Dispose();
            }
            catch { }
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
