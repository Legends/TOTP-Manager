using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TOTP.Commands;
using TOTP.Enums;
using TOTP.Interfaces;

namespace TOTP.ViewModels;

public class UserMessageDialogViewModel : INotifyPropertyChanged, IUserMessageDialogViewModel
{
    private CaptionType _caption = CaptionType.Info;
    private string? _message;
    private string? _iconPath;
    private Brush _titleBarBackground = Brushes.Gray;
    private Brush _titleBarForeground = Brushes.White;
    private string _okButtonText = "OK";
    private string _cancelButtonText = "Cancel";
    private bool _showCancelButton = false;

    public UserMessageDialogViewModel()
    {
        OkCommand = new RelayCommand(_ => OnOk());
        CancelCommand = new RelayCommand(_ => OnCancel(), () => ShowCancelButton);
    }

    public CaptionType Caption
    {
        get => _caption;
        set { _caption = value; OnPropertyChanged(); }
    }

    public string? Message
    {
        get => _message;
        set { _message = value; OnPropertyChanged(); }
    }

    public string? IconPath
    {
        get => _iconPath;
        set { _iconPath = value; OnPropertyChanged(); OnPropertyChanged(nameof(IconVisibility)); }
    }

    public Brush TitleBarBackground
    {
        get => _titleBarBackground;
        set { _titleBarBackground = value; OnPropertyChanged(); }
    }

    public Brush TitleBarForeground
    {
        get => _titleBarForeground;
        set { _titleBarForeground = value; OnPropertyChanged(); }
    }

    public string OkButtonText
    {
        get => _okButtonText;
        set { _okButtonText = value; OnPropertyChanged(); }
    }

    public string CancelButtonText
    {
        get => _cancelButtonText;
        set { _cancelButtonText = value; OnPropertyChanged(); }
    }

    public bool ShowCancelButton
    {
        get => _showCancelButton;
        set { _showCancelButton = value; OnPropertyChanged(); OnPropertyChanged(nameof(CancelButtonVisibility)); }
    }

    public ICommand OkCommand { get; }
    public ICommand CancelCommand { get; }

    public event EventHandler<bool>? RequestClose;

    private void OnOk()
    {
        RequestClose?.Invoke(this, true);
    }

    private void OnCancel()
    {
        RequestClose?.Invoke(this, false);
    }

    public Visibility IconVisibility => string.IsNullOrEmpty(IconPath) ? Visibility.Collapsed : Visibility.Visible;
    public Visibility CancelButtonVisibility => ShowCancelButton ? Visibility.Visible : Visibility.Collapsed;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
