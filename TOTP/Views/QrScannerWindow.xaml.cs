using Syncfusion.Windows.Shared;
using System.ComponentModel;
using System.Windows;
using TOTP.ViewModels;

namespace TOTP.Views
{
    public partial class QrScannerWindow : ChromelessWindow
    {
        private readonly QrScannerViewModel _vm;

        public string? DecodedText => _vm.DecodedText;

        public QrScannerWindow(QrScannerViewModel vm)
        {
            InitializeComponent();

            _vm = vm;
            DataContext = _vm;

            _vm.CloseRequested += Vm_CloseRequested;

            Loaded += (_, __) => _vm.Start();
            Closing += OnClosing;
        }

        private void Vm_CloseRequested(object? sender, CloseRequestedEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => Vm_CloseRequested(sender, e));
                return;
            }

            DialogResult = e.DialogResult;
            Close();
        }

        private void OnClosing(object? sender, CancelEventArgs e)
        {
            _vm.CloseRequested -= Vm_CloseRequested;
            _vm.Stop();
            _vm.Dispose();
        }
    }
}
