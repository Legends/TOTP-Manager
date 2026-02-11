using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace TOTP.UserControls;

public partial class FlyoutHost : UserControl
{
    public FlyoutHost()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(
            nameof(IsOpen),
            typeof(bool),
            typeof(FlyoutHost),
            new FrameworkPropertyMetadata(false));

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public static readonly DependencyProperty FlyoutContentProperty =
        DependencyProperty.Register(nameof(FlyoutContent), typeof(object), typeof(FlyoutHost),
            new FrameworkPropertyMetadata(null, OnFlyoutContentChanged));

    public object? FlyoutContent
    {
        get => GetValue(FlyoutContentProperty);
        set => SetValue(FlyoutContentProperty, value);
    }

    private static void OnFlyoutContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var host = (FlyoutHost)d;
        host.ApplyInheritedDataContext(e.NewValue);
    }

    private void ApplyInheritedDataContext(object? content)
    {
        if (content is not FrameworkElement element)
        {
            return;
        }

        if (element.ReadLocalValue(DataContextProperty) != DependencyProperty.UnsetValue)
        {
            return;
        }

        BindingOperations.SetBinding(
            element,
            DataContextProperty,
            new Binding(nameof(DataContext)) { Source = this, Mode = BindingMode.OneWay });
    }

    public static readonly DependencyProperty CloseCommandProperty =
        DependencyProperty.Register(nameof(CloseCommand), typeof(ICommand), typeof(FlyoutHost),
            new FrameworkPropertyMetadata(null));

    public ICommand? CloseCommand
    {
        get => (ICommand?)GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }

    public static readonly DependencyProperty PanelWidthProperty =
        DependencyProperty.Register(nameof(PanelWidth), typeof(double), typeof(FlyoutHost),
            new FrameworkPropertyMetadata(300d));

    public double PanelWidth
    {
        get => (double)GetValue(PanelWidthProperty);
        set => SetValue(PanelWidthProperty, value);
    }

    public static readonly DependencyProperty OverlayBrushProperty =
        DependencyProperty.Register(nameof(OverlayBrush), typeof(Brush), typeof(FlyoutHost),
            new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromArgb(0x66, 0, 0, 0))));

    public Brush OverlayBrush
    {
        get => (Brush)GetValue(OverlayBrushProperty);
        set => SetValue(OverlayBrushProperty, value);
    }

    public static readonly DependencyProperty PanelBackgroundProperty =
        DependencyProperty.Register(nameof(PanelBackground), typeof(Brush), typeof(FlyoutHost),
            new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(0x15, 0x25, 0x49))));

    public Brush PanelBackground
    {
        get => (Brush)GetValue(PanelBackgroundProperty);
        set => SetValue(PanelBackgroundProperty, value);
    }

    public static readonly DependencyProperty PanelPaddingProperty =
        DependencyProperty.Register(nameof(PanelPadding), typeof(Thickness), typeof(FlyoutHost),
            new FrameworkPropertyMetadata(new Thickness(16)));

    public Thickness PanelPadding
    {
        get => (Thickness)GetValue(PanelPaddingProperty);
        set => SetValue(PanelPaddingProperty, value);
    }

    public static readonly DependencyProperty PanelBorderBrushProperty =
        DependencyProperty.Register(nameof(PanelBorderBrush), typeof(Brush), typeof(FlyoutHost),
            new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(0x39, 0x4B, 0x74))));

    public Brush PanelBorderBrush
    {
        get => (Brush)GetValue(PanelBorderBrushProperty);
        set => SetValue(PanelBorderBrushProperty, value);
    }

    public static readonly DependencyProperty PanelBorderThicknessProperty =
        DependencyProperty.Register(nameof(PanelBorderThickness), typeof(Thickness), typeof(FlyoutHost),
            new FrameworkPropertyMetadata(new Thickness(1)));

    public Thickness PanelBorderThickness
    {
        get => (Thickness)GetValue(PanelBorderThicknessProperty);
        set => SetValue(PanelBorderThicknessProperty, value);
    }

    public static readonly DependencyProperty OverlayZIndexProperty =
        DependencyProperty.Register(nameof(OverlayZIndex), typeof(int), typeof(FlyoutHost),
            new FrameworkPropertyMetadata(20));

    public int OverlayZIndex
    {
        get => (int)GetValue(OverlayZIndexProperty);
        set => SetValue(OverlayZIndexProperty, value);
    }

    public static readonly DependencyProperty PanelZIndexProperty =
        DependencyProperty.Register(nameof(PanelZIndex), typeof(int), typeof(FlyoutHost),
            new FrameworkPropertyMetadata(21));

    public int PanelZIndex
    {
        get => (int)GetValue(PanelZIndexProperty);
        set => SetValue(PanelZIndexProperty, value);
    }
}
