using System;
using System.Windows;
using TOTP.Services.Interfaces;
using TOTP.Views;

namespace TOTP.Services
{
    public sealed class QrScannerDialogService : IQrScannerDialogService
    {
        private readonly Func<QrScannerWindow> _windowFactory;

        public QrScannerDialogService(Func<QrScannerWindow> windowFactory)
            => _windowFactory = windowFactory;

        public string? ScanQrCode()
        {
            var win = _windowFactory();
            win.Owner = Application.Current?.MainWindow;

            var ok = win.ShowDialog() == true;
            return ok ? win.DecodedText : null;
        }
    }
}
