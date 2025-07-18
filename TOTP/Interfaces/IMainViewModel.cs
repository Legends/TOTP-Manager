using Github2FA.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

public interface IMainViewModel : INotifyPropertyChanged
{
    ObservableCollection<SecretItem> AllSecrets { get; }
    ICommand AddNewTotpCommand { get; }
    ICommand DeleteSecretCommand { get; }
    ICommand UpdateSecretCommand { get; }
    ICommand BeginEditCommand { get; }
    ICommand EndEditCommand { get; }
    ICommand DoubleClickCommand { get; }
    SecretItem? SelectedSecret { get; set; }
    bool ShowActionsColumn { get; }
    string? CurrentCodeLabel { get; }
    bool IsCodeCopiedVisible { get; }
    SecretItem? PreviousVersion { get; set; } // ADD THIS

    void UpdateSecret(SecretItem updated); // ADD THIS
    bool IsContextmenuOpen { get; set; }



}
