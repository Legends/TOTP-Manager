using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;


namespace TOTP.ViewModels;

public class DialogViewModel : INotifyPropertyChanged
{

    public DialogViewModel()
    {

    }
    public string Message { get; set; } = string.Empty;
    public string Caption { get; set; } = "Message";

    public string OkButtonText { get; set; } = "OK";
    public string CancelButtonText { get; set; } = "Cancel";

    public bool ShowCancelButton { get; set; }

    public string? IconPath { get; set; }
    public Brush? TitleBarBackground { get; set; } =
        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF7D7FF4"));

    public Brush? TitleBarForeground { get; set; } = Brushes.White;

    public Visibility IconVisibility => string.IsNullOrEmpty(IconPath) ? Visibility.Collapsed : Visibility.Visible;
    public Visibility CancelButtonVisibility => ShowCancelButton ? Visibility.Visible : Visibility.Collapsed;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}