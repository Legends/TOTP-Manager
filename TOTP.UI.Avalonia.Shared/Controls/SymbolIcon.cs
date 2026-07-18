using Avalonia;
using Avalonia.Controls.Primitives;

namespace TOTP.Avalonia.Shared.Controls;

public sealed class SymbolIcon : TemplatedControl
{
    private string _iconData = GeometryFor(SymbolIconKind.Add);

    public static readonly StyledProperty<SymbolIconKind> KindProperty =
        AvaloniaProperty.Register<SymbolIcon, SymbolIconKind>(nameof(Kind), SymbolIconKind.Add);

    public static readonly StyledProperty<double> IconSizeProperty =
        AvaloniaProperty.Register<SymbolIcon, double>(nameof(IconSize), 18d);

    public static readonly DirectProperty<SymbolIcon, string> IconDataProperty =
        AvaloniaProperty.RegisterDirect<SymbolIcon, string>(
            nameof(IconData),
            static control => control.IconData);

    static SymbolIcon()
    {
        KindProperty.Changed.AddClassHandler<SymbolIcon>(
            static (control, _) => control.IconData = GeometryFor(control.Kind));
    }

    public SymbolIconKind Kind
    {
        get => GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    public double IconSize
    {
        get => GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    public string IconData
    {
        get => _iconData;
        private set => SetAndRaise(IconDataProperty, ref _iconData, value);
    }

    private static string GeometryFor(SymbolIconKind kind) => kind switch
    {
        SymbolIconKind.Add => "M11,5 L13,5 L13,11 L19,11 L19,13 L13,13 L13,19 L11,19 L11,13 L5,13 L5,11 L11,11 Z",
        SymbolIconKind.Camera => "M9,3 L7.2,5 L4,5 C2.9,5 2,5.9 2,7 L2,19 C2,20.1 2.9,21 4,21 L20,21 C21.1,21 22,20.1 22,19 L22,7 C22,5.9 21.1,5 20,5 L16.8,5 L15,3 Z M12,8 C15.3,8 18,10.7 18,14 C18,17.3 15.3,20 12,20 C8.7,20 6,17.3 6,14 C6,10.7 8.7,8 12,8 Z M12,10 C9.8,10 8,11.8 8,14 C8,16.2 9.8,18 12,18 C14.2,18 16,16.2 16,14 C16,11.8 14.2,10 12,10 Z",
        SymbolIconKind.Conceal => "M2,4.3 L3.4,3 L21,20.6 L19.6,22 L16.7,19.1 C15.2,19.7 13.6,20 12,20 C6.5,20 2,16.6 0,12 C0.9,10 2.2,8.2 3.9,6.9 Z M7.1,10.1 C7,10.4 7,10.7 7,11 C7,13.8 9.2,16 12,16 C12.3,16 12.6,16 12.9,15.9 Z M12,4 C17.5,4 22,7.4 24,12 C23.2,13.8 22,15.4 20.6,16.7 L17.8,13.9 C17.9,13.6 18,13.3 18,13 C18,9.7 15.3,7 12,7 C11.7,7 11.4,7.1 11.1,7.2 L8.5,4.6 C9.6,4.2 10.8,4 12,4 Z",
        SymbolIconKind.Copy => "M8,2 L19,2 C20.1,2 21,2.9 21,4 L21,16 L19,16 L19,4 L8,4 Z M5,6 L16,6 C17.1,6 18,6.9 18,8 L18,20 C18,21.1 17.1,22 16,22 L5,22 C3.9,22 3,21.1 3,20 L3,8 C3,6.9 3.9,6 5,6 Z M5,8 L5,20 L16,20 L16,8 Z",
        SymbolIconKind.Lock => "M7,10 L7,7 C7,4.2 9.2,2 12,2 C14.8,2 17,4.2 17,7 L17,10 L19,10 C20.1,10 21,10.9 21,12 L21,21 C21,22.1 20.1,23 19,23 L5,23 C3.9,23 3,22.1 3,21 L3,12 C3,10.9 3.9,10 5,10 Z M9,10 L15,10 L15,7 C15,5.3 13.7,4 12,4 C10.3,4 9,5.3 9,7 Z",
        SymbolIconKind.Reveal => "M12,4 C17.5,4 22,7.4 24,12 C22,16.6 17.5,20 12,20 C6.5,20 2,16.6 0,12 C2,7.4 6.5,4 12,4 Z M12,7 C9.2,7 7,9.2 7,12 C7,14.8 9.2,17 12,17 C14.8,17 17,14.8 17,12 C17,9.2 14.8,7 12,7 Z M12,9 C13.7,9 15,10.3 15,12 C15,13.7 13.7,15 12,15 C10.3,15 9,13.7 9,12 C9,10.3 10.3,9 12,9 Z",
        SymbolIconKind.Search => "M10.5,3 C14.6,3 18,6.4 18,10.5 C18,12.1 17.5,13.6 16.7,14.8 L22,20.1 L20.1,22 L14.8,16.7 C13.6,17.5 12.1,18 10.5,18 C6.4,18 3,14.6 3,10.5 C3,6.4 6.4,3 10.5,3 Z M10.5,5.5 C7.7,5.5 5.5,7.7 5.5,10.5 C5.5,13.3 7.7,15.5 10.5,15.5 C13.3,15.5 15.5,13.3 15.5,10.5 C15.5,7.7 13.3,5.5 10.5,5.5 Z",
        SymbolIconKind.Settings => "M10.9,2 L13.1,2 L13.8,4.2 C14.4,4.4 15,4.6 15.5,4.9 L17.6,3.9 L19.1,5.4 L18.1,7.5 C18.4,8 18.6,8.6 18.8,9.2 L21,9.9 L21,12.1 L18.8,12.8 C18.6,13.4 18.4,14 18.1,14.5 L19.1,16.6 L17.6,18.1 L15.5,17.1 C15,17.4 14.4,17.6 13.8,17.8 L13.1,20 L10.9,20 L10.2,17.8 C9.6,17.6 9,17.4 8.5,17.1 L6.4,18.1 L4.9,16.6 L5.9,14.5 C5.6,14 5.4,13.4 5.2,12.8 L3,12.1 L3,9.9 L5.2,9.2 C5.4,8.6 5.6,8 5.9,7.5 L4.9,5.4 L6.4,3.9 L8.5,4.9 C9,4.6 9.6,4.4 10.2,4.2 Z M12,7 C9.8,7 8,8.8 8,11 C8,13.2 9.8,15 12,15 C14.2,15 16,13.2 16,11 C16,8.8 14.2,7 12,7 Z",
        _ => "M11,5 L13,5 L13,11 L19,11 L19,13 L13,13 L13,19 L11,19 L11,13 L5,13 L5,11 L11,11 Z"
    };
}
