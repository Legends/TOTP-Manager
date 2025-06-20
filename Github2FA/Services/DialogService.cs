using Github2FA.Interfaces;
using Github2FA.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
