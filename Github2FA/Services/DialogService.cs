using Github2FA.Interfaces;
using Github2FA.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Github2FA.Services
{
    public class DialogService : IDialogService
    {
        public (bool success, string? key, string? value) ShowKeyValueDialog()
        {
            var dlg = new KeyValueDialog
            {
                Owner = System.Windows.Application.Current?.MainWindow
            };
            var result = dlg.ShowDialog() == true;
            return (result, dlg.ViewModel.Key, dlg.ViewModel.Value);
        }

        public (bool success, string? key, string? value) ShowKeyValueDialog(string? initialKey = null, string? initialValue = null)
        {
            var dlg = new KeyValueDialog
            {
                Owner = Application.Current?.MainWindow
            };

            if (initialKey != null)
                dlg.ViewModel.Key = initialKey;

            if (initialValue != null)
                dlg.ViewModel.Value = initialValue;

            var result = dlg.ShowDialog() == true;
            return (result, dlg.ViewModel.Key, dlg.ViewModel.Value);
        }

    }
}
