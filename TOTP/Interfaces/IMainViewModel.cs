using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using TOTP.ViewModels;

namespace TOTP.Interfaces;

public interface IMainViewModel : INotifyPropertyChanged
{
    ObservableCollection<SecretItemViewModel> AllSecrets { get; }
    //ICommand AddNewSecretCommand { get; }
    //ICommand DeleteSecretCommand { get; }
    //ICommand UpdateSecretCommand { get; }
    //ICommand BeginEditCommand { get; }
    //ICommand EndEditCommand { get; }
    //ICommand DoubleClickCommand { get; }
    Task AddNewSecretAsync();
    SecretItemViewModel SelectedSecret { get; set; }
    //bool ShowActionsColumn { get; }
    //string? CurrentCodeLabel { get; }
    //bool IsCodeCopiedVisible { get; }
    //SecretItem? PreviousVersion { get; set; } // ADD THIS
    bool IsContextmenuOpen { get; set; }
    Task InitializeAsync();
    Task OnSelectionChangedAsync();
    //void UpdateSecret(SecretItem updated); // ADD THIS
}