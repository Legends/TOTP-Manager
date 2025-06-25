using Github2FA.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace Github2FA.Interfaces
{
    public interface IMainViewModel
    {
        event PropertyChangedEventHandler? PropertyChanged;
        ObservableCollection<SecretItem> Secrets { get; }

        ICommand AddNewTotpCommand { get; }
        ICommand DeleteSecretCommand { get; }
        ICommand UpdateSecretCommand { get; }
        SecretItem PreviousVersion { get; set; }
        bool ShowActionsColumn { get; }
        void UpdateSecret(SecretItem original, SecretItem updated);
    }
}