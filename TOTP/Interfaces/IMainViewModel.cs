using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using TOTP.Commands;
using TOTP.ViewModels;

namespace TOTP.Interfaces;

public interface IMainViewModel : INotifyPropertyChanged
{
    ObservableCollection<SecretItemViewModel> AllSecrets { get; }
    AsyncCommand AddNewSecretCommand { get; }
    ICommand DeleteSecretCommand { get; }
    ICommand UpdateSecretCommand { get; }
    ICommand BeginEditCommand { get; }
    ICommand EndEditCommand { get; }
    ICommand DoubleClickCommand { get; }
    Task AddNewSecretAsync();
    SecretItemViewModel SelectedSecret { get; set; }

    bool IsContextmenuOpen { get; set; }
    Task InitializeAsync();
    Task OnSelectionChangedAsync();

}