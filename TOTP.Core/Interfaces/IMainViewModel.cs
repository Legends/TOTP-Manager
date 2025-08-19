using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using TOTP.Core.Models;

namespace TOTP.Core.Interfaces;

public interface IMainViewModel : INotifyPropertyChanged
{
    ObservableCollection<SecretItem> AllSecrets { get; }
    //ICommand AddNewSecretCommand { get; }
    //ICommand DeleteSecretCommand { get; }
    //ICommand UpdateSecretCommand { get; }
    //ICommand BeginEditCommand { get; }
    //ICommand EndEditCommand { get; }
    //ICommand DoubleClickCommand { get; }
    Task AddNewSecretAsync();
    SecretItem SelectedSecret { get; set; }
    //bool ShowActionsColumn { get; }
    //string? CurrentCodeLabel { get; }
    //bool IsCodeCopiedVisible { get; }
    //SecretItem? PreviousVersion { get; set; } // ADD THIS
    bool IsContextmenuOpen { get; set; }
    Task InitializeAsync();
    Task OnSelectionChangedAsync();
    //void UpdateSecret(SecretItem updated); // ADD THIS
}