using Syncfusion.Windows.Shared;
using System;
using System.Threading.Tasks;
using System.Windows;
using TOTP.Resources;
using TOTP.ViewModels;

namespace TOTP.Views;

public partial class PasswordPromptWindow : ChromelessWindow
{
    public Func<string, Task<string?>>? ValidatePasswordAsync { get; set; }

    public PasswordPromptWindow()
    {
        InitializeComponent();
    }

    private async void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is PasswordPromptViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Password))
            {
                vm.ErrorMessage = string.IsNullOrWhiteSpace(vm.RequiredErrorMessage)
                    ? UI.ui_ImportPasswordRequired
                    : vm.RequiredErrorMessage;
                return;
            }

            if (ValidatePasswordAsync != null)
            {
                var validationError = await ValidatePasswordAsync(vm.Password);
                if (!string.IsNullOrWhiteSpace(validationError))
                {
                    vm.ErrorMessage = validationError;
                    return;
                }
            }
        }

        DialogResult = true;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        CenterWithinOwnerOrScreen();
        Opacity = 1d;
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
