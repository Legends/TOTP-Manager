using Syncfusion.Windows.Shared;
using TOTP.Interfaces;
using TOTP.Resources;

//ControlNamespace###

namespace TOTP.Windows;

/// <summary>
///     Interaction logic for KeyValueDialog.xaml
/// </summary>
public partial class KeyValueDialog : ChromelessWindow
{
    public KeyValueDialog(IPlatformSecretDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Title = UI.ui_Window_Title_PlatformSecretDialog;

        viewModel.RequestClose += ViewModel_RequestClose;
        Closed += (_, _) => ViewModel.RequestClose -= ViewModel_RequestClose;
    }

    private void ViewModel_RequestClose(object? sender, bool e)
    {
        DialogResult = e;
        //Close();
    }

    public IPlatformSecretDialogViewModel ViewModel => (IPlatformSecretDialogViewModel)DataContext;

}