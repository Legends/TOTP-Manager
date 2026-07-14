using System;
using System.Windows;
using TOTP.ViewModels;

namespace TOTP.Views;

public partial class PasswordPromptWindow : CenteredChromelessWindow
{
    public PasswordPromptWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is PasswordPromptViewModel oldVm)
        {
            oldVm.RequestClose -= OnViewModelRequestClose;
        }

        if (e.NewValue is PasswordPromptViewModel newVm)
        {
            newVm.RequestClose += OnViewModelRequestClose;
        }
    }

    private void OnViewModelRequestClose(object? sender, EventArgs e)
    {
        DialogResult = true;
    }

    protected override void OnClosed(EventArgs e)
    {
        if (DataContext is PasswordPromptViewModel vm)
        {
            vm.RequestClose -= OnViewModelRequestClose;
            vm.ClearSensitiveData();
        }

        DataContextChanged -= OnDataContextChanged;
        base.OnClosed(e);
    }

}
