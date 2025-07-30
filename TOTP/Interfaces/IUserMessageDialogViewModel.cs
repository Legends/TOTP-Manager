using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TOTP.Enums;
using TOTP.Events;

namespace TOTP.Interfaces
{
    public interface IUserMessageDialogViewModel : INotifyPropertyChanged
    {
        CaptionType Caption { get; set; }
        string? Message { get; set; }
        string? IconPath { get; set; }
        Brush TitleBarBackground { get; set; }
        Brush TitleBarForeground { get; set; }
        string OkButtonText { get; set; }
        string CancelButtonText { get; set; }
        bool ShowCancelButton { get; set; }

        ICommand OkCommand { get; }
        ICommand CancelCommand { get; }

        Visibility IconVisibility { get; }
        Visibility CancelButtonVisibility { get; }

        event EventHandler<DialogCloseRequestedEventArgs> RequestClose;
    }

}
