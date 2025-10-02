using System;
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
    RelayCommand OpenFlyoutAddModeCommand { get; }
    ICommand DeleteSecretCommand { get; }
    ICommand UpdateSecretCommand { get; }
    ICommand BeginEditCommand { get; }
    ICommand EndEditCommand { get; }
    ICommand DoubleClickCommand { get; }
    void OpenFlyoutAddMode();
    SecretItemViewModel SelectedSecret { get; set; }
    Action? RequestGridFilterRefresh { get; set; }
    bool DoFilterGrid(object obj);
    bool IsContextmenuOpen { get; set; }
    Task InitializeAsync();
    Task OnRowSelectionChangedAsync();

}