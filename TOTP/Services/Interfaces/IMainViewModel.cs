using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using TOTP.Commands;
using TOTP.ViewModels;

namespace TOTP.Services.Interfaces;

public interface IMainViewModel : INotifyPropertyChanged
{
    ObservableCollection<AccountViewModel> AllSecrets { get; }
    RelayCommand OpenFlyoutAddModeCommand { get; }
    ICommand DeleteSecretCommand { get; }
    ICommand UpdateSecretCommand { get; }
    ICommand BeginEditCommand { get; }
    ICommand EndEditCommand { get; }
    ICommand DoubleClickCommand { get; }
    void OpenFlyoutAddMode();
    AccountViewModel SelectedSecret { get; set; }
    Action? RequestGridFilterRefresh { get; set; }
    bool DoFilterGrid(object obj);
    bool IsContextmenuOpen { get; set; }
    Task InitializeAsync();
    Task OnRowSelectionChangedAsync(AccountViewModel item);
    bool IsGridEditing { get; set; }
    bool IsInlineEditing { get; set; }
    bool IsSecretVisible { get; set; }

}