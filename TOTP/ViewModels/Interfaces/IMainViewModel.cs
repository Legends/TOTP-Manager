using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using TOTP.Commands;
using TOTP.Core.Interfaces;
using TOTP.Infrastructure.Adapters;
using TOTP.ViewModels;
using TOTP.Views.Interfaces;

namespace TOTP.ViewModels.Interfaces;

public interface IMainViewModel : INotifyPropertyChanged
{
    bool IsBusy { get; set; }
    ObservableCollection<OtpViewModel> AllOtps { get; }
    IGridFilterRefresher GridFilterRefresher { get; set; }
    Action? RequestGridFilterRefresh { get; set; }
    bool DoFilterGrid(object obj);
    RelayCommand OpenFlyoutAddModeCommand { get; }
    ICommand DeleteSecretCommand { get; }
    ICommand UpdateSecretCommand { get; }
    ICommand BeginEditCommand { get; }
    ICommand EndEditCommand { get; }
    ICommand DoubleClickCommand { get; }
    void OpenFlyoutAddMode();
    OtpViewModel? SelectedToken { get; set; }
    bool IsContextmenuOpen { get; set; }
    Task InitializeMainViewAsync(IMainWindow? mainWindow);
    Task OnRowSelectionChangedAsync(OtpViewModel? item);
    bool IsGridEditing { get; set; }
    bool IsInlineEditing { get; set; }
    bool IsSecretVisible { get; set; }

}
