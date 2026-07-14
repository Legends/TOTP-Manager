using System;
using System.Windows;
using TOTP.ViewModels;

namespace TOTP.Views;

public partial class ExportPasswordPromptWindow : CenteredChromelessWindow
{
    public ExportPasswordPromptWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    protected override void OnClosed(EventArgs e)
    {
        if (DataContext is ExportPasswordPromptViewModel vm)
        {
            vm.RequestClose -= OnViewModelRequestClose;
            vm.ClearSensitiveData();
        }

        DataContextChanged -= OnDataContextChanged;
        base.OnClosed(e);
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is ExportPasswordPromptViewModel oldVm)
        {
            oldVm.RequestClose -= OnViewModelRequestClose;
        }

        if (e.NewValue is ExportPasswordPromptViewModel newVm)
        {
            newVm.RequestClose += OnViewModelRequestClose;
        }
    }

    private void OnViewModelRequestClose(object? sender, EventArgs e)
    {
        DialogResult = true;
    }

}
