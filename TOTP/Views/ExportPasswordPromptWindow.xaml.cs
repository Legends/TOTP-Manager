using Syncfusion.Windows.Shared;
using System;
using System.Windows;
using System.Windows.Threading;
using TOTP.Resources;
using TOTP.ViewModels;
using System.Threading.Tasks;

namespace TOTP.Views;

public partial class ExportPasswordPromptWindow : ChromelessWindow
{
    public string? SelectedPassword { get; private set; }
    public Func<string, Task<bool>>? ValidateMasterPasswordAsync { get; set; }

    public ExportPasswordPromptWindow()
    {
        InitializeComponent();
    }

    private async void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ExportPasswordPromptViewModel vm)
        {
            return;
        }

        vm.ErrorMessage = string.Empty;

        if (vm.UseMasterPassword)
        {
            if (string.IsNullOrWhiteSpace(vm.MasterPassword))
            {
                vm.ErrorMessage = UI.ui_ExportPasswordRequired;
                return;
            }

            if (ValidateMasterPasswordAsync != null)
            {
                var isValid = await ValidateMasterPasswordAsync(vm.MasterPassword);
                if (!isValid)
                {
                    vm.ErrorMessage = UI.ui_ExportPwd_WrongMasterPassword;
                    return;
                }
            }

            SelectedPassword = vm.MasterPassword;
            DialogResult = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(vm.CustomPassword) || string.IsNullOrWhiteSpace(vm.ConfirmCustomPassword))
        {
            vm.ErrorMessage = UI.ui_ExportPasswordRequired;
            return;
        }

        if (!string.Equals(vm.CustomPassword, vm.ConfirmCustomPassword, StringComparison.Ordinal))
        {
            vm.ErrorMessage = UI.ui_ExportPwd_CustomPasswordMismatch;
            return;
        }

        SelectedPassword = vm.CustomPassword;
        DialogResult = true;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(new Action(CenterWithinOwnerOrScreen), DispatcherPriority.ApplicationIdle);
    }

    private void OnContentRendered(object? sender, EventArgs e)
    {
        CenterWithinOwnerOrScreen();
    }

    private void CenterWithinOwnerOrScreen()
    {
        if (ActualWidth <= 0 || ActualHeight <= 0)
        {
            UpdateLayout();
        }

        var target = GetTargetBounds();
        var left = target.Left + ((target.Width - ActualWidth) / 2d);
        var top = target.Top + ((target.Height - ActualHeight) / 2d);

        Left = left;
        Top = top;
    }

    private Rect GetTargetBounds()
    {
        if (Owner is { IsVisible: true } owner)
        {
            var ownerWidth = owner.ActualWidth > 0 ? owner.ActualWidth : owner.Width;
            var ownerHeight = owner.ActualHeight > 0 ? owner.ActualHeight : owner.Height;

            if (ownerWidth > 0 && ownerHeight > 0 && !double.IsNaN(ownerWidth) && !double.IsNaN(ownerHeight))
            {
                return new Rect(owner.Left, owner.Top, ownerWidth, ownerHeight);
            }

            var restoreBounds = owner.RestoreBounds;
            if (restoreBounds.Width > 0 && restoreBounds.Height > 0)
            {
                return restoreBounds;
            }
        }

        return new Rect(SystemParameters.WorkArea.Left, SystemParameters.WorkArea.Top, SystemParameters.WorkArea.Width, SystemParameters.WorkArea.Height);
    }
}
