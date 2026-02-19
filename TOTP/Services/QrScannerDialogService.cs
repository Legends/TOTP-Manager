using System;
using Microsoft.Extensions.DependencyInjection;
using TOTP.Services.Interfaces;
using TOTP.Views;

namespace TOTP.Services
{
    public sealed class QrScannerDialogService : IQrScannerDialogService
    {
        private readonly IServiceProvider _sp;

        public QrScannerDialogService(IServiceProvider sp) => _sp = sp;

        public string? ScanQrCode(System.Windows.Window owner)
        {
            // Resolve a fresh window (and therefore a fresh VM) from DI
            var win = _sp.GetRequiredService<QrScannerWindow>();
            win.Owner = owner;

            var ok = win.ShowDialog() == true;
            return ok ? win.DecodedText : null;
        }
    }
}