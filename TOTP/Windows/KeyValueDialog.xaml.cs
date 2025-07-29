using Syncfusion.Windows.Shared;
using TOTP.Interfaces;

//ControlNamespace###

namespace TOTP.Windows;

/// <summary>
///     Interaction logic for KeyValueDialog.xaml
/// </summary>
public partial class KeyValueDialog : ChromelessWindow
{
    public KeyValueDialog(IKeyValueDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        viewModel.RequestClose += ViewModel_RequestClose;
        Closed += (_, _) => ViewModel.RequestClose -= ViewModel_RequestClose;
    }

    private void ViewModel_RequestClose(object? sender, bool e)
    {
        DialogResult = e;
        //Close();
    }

    public IKeyValueDialogViewModel ViewModel => (IKeyValueDialogViewModel)DataContext;

}