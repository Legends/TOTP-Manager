using FluentResults;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using TOTP.Core.Common;
using TOTP.Core.Enums;
using TOTP.Extensions;
using TOTP.Helper;
using TOTP.Resources;
using TOTP.Services.Interfaces;
using TOTP.ViewModels;
using TOTP.Views;

namespace TOTP.Services;

public class MessageService(Func<IUserMessageDialogViewModel> vmFactory) : IMessageService
{
 

    public void ShowResultError(IResultBase result, string? platform = null)
    {
        if (result.IsSuccess) return;

        var status = result.GetStatus();
        var localizedMsg = status.ToLocalizedMessage(platform, result.GetFullMessage());

        ShowError(localizedMsg);
    }

   

    #region Notification API

    public void ShowInfo(string msg) => Show(msg, CaptionType.Info);
    public void ShowWarning(string msg) => Show(msg, CaptionType.Warning);
    public void ShowError(string msg) => Show(msg, CaptionType.Error);

    public void ShowMessage(string message, CaptionType caption = CaptionType.Default, string iconPath = "")
        => Show(message, caption, iconPath);

    #endregion

    #region Confirmation API

    public bool ConfirmInfo(string msg, string? ok = null, string? cancel = null)
        => ShowDialog(msg, CaptionType.Info, ok, cancel);

    public bool ConfirmWarning(string msg, string? ok = null, string? cancel = null)
        => ShowDialog(msg, CaptionType.Warning, ok, cancel);

    public bool ConfirmError(string msg, string? ok = null, string? cancel = null)
        => ShowDialog(msg, CaptionType.Error, ok, cancel);

    public bool ShowMessageDialog(string message, CaptionType caption = CaptionType.Default, string iconPath = "")
        => ShowDialog(message, caption, null, null, iconPath);

    #endregion

    #region Private Core Logic

    private void Show(string msg, CaptionType type, string iconPath = "")
        => ShowCore(msg, type, showCancel: false, 0.65, null, null, iconPath);

    private bool ShowDialog(string msg, CaptionType type, string? ok = null, string? cancel = null, string iconPath = "")
        => ShowCore(msg, type, showCancel: true, 0.55, ok, cancel, iconPath);

    private bool ShowCore(string message, CaptionType caption, bool showCancel, double dimOpacity, string? ok, string? cancel, string iconPath)
    {
        var vm = vmFactory();
        ConfigureViewModel(vm, message, caption, showCancel, ok, cancel, iconPath);

        var owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive)
                    ?? Application.Current.MainWindow;

        var dialog = new UserMessageDialog(vm) { Owner = owner };

        if (dialog.Owner is null)
        {
            dialog.ShowDialog();
            return dialog.Result;
        }

        var oldOpacity = dialog.Owner.Opacity;
        var oldEffect = dialog.Owner.Effect;

        try
        {
            dialog.Owner.Opacity = dimOpacity;
            dialog.Owner.Effect = new BlurEffect { Radius = 6 };
            dialog.ShowDialog();
            return dialog.Result;
        }
        finally
        {
            dialog.Owner.Opacity = oldOpacity;
            dialog.Owner.Effect = oldEffect;
        }
    }

    private void ConfigureViewModel(IUserMessageDialogViewModel vm, string message, CaptionType caption, bool showCancel, string? ok, string? cancel, string customIcon)
    {
        vm.Message = message;
        vm.Caption = caption;
        vm.ShowCancelButton = showCancel;
        vm.OkButtonText = ok ?? UI.ui_btnOK;
        vm.CancelButtonText = cancel ?? UI.ui_btnCancel;

        // Visual Mapping
        (var standardIcon, vm.TitleBarBackground, vm.TitleBarForeground) = caption switch
        {
            CaptionType.Error => (StringsConstants.ImgUrl.ImgError, Brushes.Red, Brushes.White),
            CaptionType.Warning => (StringsConstants.ImgUrl.ImgWarning, Brushes.LightYellow, Brushes.Black),
            CaptionType.Info => (StringsConstants.ImgUrl.ImgInfo, Brushes.LightSteelBlue, Brushes.White),
            _ => (string.Empty, Brushes.Gray, Brushes.White)
        };

        // If a custom icon path was passed via ShowMessage/ShowMessageDialog, use it; otherwise use the standard one.
        vm.IconPath = !string.IsNullOrEmpty(customIcon) ? customIcon : standardIcon;
    }

    #endregion
}