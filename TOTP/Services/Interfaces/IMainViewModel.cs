using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Data;
using TOTP.Commands;
using TOTP.ViewModels;

namespace TOTP.Services.Interfaces;

public interface IMainViewModel : INotifyPropertyChanged
{
    ObservableCollection<AccountViewModel> AllSecrets { get; }
    ICollectionView FilteredSecrets { get; }
    RelayCommand OpenFlyoutAddModeCommand { get; }
    ICommand DeleteSecretCommand { get; }
    ICommand UpdateSecretCommand { get; }
    ICommand BeginEditCommand { get; }
    ICommand EndEditCommand { get; }
    ICommand DoubleClickCommand { get; }
    void OpenFlyoutAddMode();
    AccountViewModel SelectedSecret { get; set; }
    bool IsContextmenuOpen { get; set; }
    Task InitializeMainViewAsync(IMainWindow? mainWindow);
    Task OnRowSelectionChangedAsync(AccountViewModel item);
    bool IsGridEditing { get; set; }
    bool IsInlineEditing { get; set; }
    bool IsSecretVisible { get; set; }

}
