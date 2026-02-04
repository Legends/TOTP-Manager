using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TOTP.UserControls
{
    public partial class RevealableSecretBox : UserControl
    {
        public RevealableSecretBox()
        {
            InitializeComponent();
        }

        // ===== Data =====

        public static readonly DependencyProperty SecretProperty =
            DependencyProperty.Register(
                nameof(Secret),
                typeof(string),
                typeof(RevealableSecretBox),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string Secret
        {
            get => (string)GetValue(SecretProperty);
            set => SetValue(SecretProperty, value);
        }

        public static readonly DependencyProperty IsSecretVisibleProperty =
            DependencyProperty.Register(
                nameof(IsSecretVisible),
                typeof(bool),
                typeof(RevealableSecretBox),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public bool IsSecretVisible
        {
            get => (bool)GetValue(IsSecretVisibleProperty);
            set => SetValue(IsSecretVisibleProperty, value);
        }

        // ===== Sizes / Layout =====

        public static readonly DependencyProperty FieldHeightProperty =
            DependencyProperty.Register(
                nameof(FieldHeight),
                typeof(double),
                typeof(RevealableSecretBox),
                new PropertyMetadata(30d));

        public double FieldHeight
        {
            get => (double)GetValue(FieldHeightProperty);
            set => SetValue(FieldHeightProperty, value);
        }

        public static readonly DependencyProperty TextPaddingProperty =
            DependencyProperty.Register(
                nameof(TextPadding),
                typeof(Thickness),
                typeof(RevealableSecretBox),
                new PropertyMetadata(new Thickness(0, 0, 6, 0)));

        public Thickness TextPadding
        {
            get => (Thickness)GetValue(TextPaddingProperty);
            set => SetValue(TextPaddingProperty, value);
        }

        public static readonly DependencyProperty ToggleButtonWidthProperty =
            DependencyProperty.Register(
                nameof(ToggleButtonWidth),
                typeof(double),
                typeof(RevealableSecretBox),
                new PropertyMetadata(32d));

        public double ToggleButtonWidth
        {
            get => (double)GetValue(ToggleButtonWidthProperty);
            set => SetValue(ToggleButtonWidthProperty, value);
        }

        public static readonly DependencyProperty ToggleButtonMarginProperty =
            DependencyProperty.Register(
                nameof(ToggleButtonMargin),
                typeof(Thickness),
                typeof(RevealableSecretBox),
                new PropertyMetadata(new Thickness(4, 0, 0, 0)));

        public Thickness ToggleButtonMargin
        {
            get => (Thickness)GetValue(ToggleButtonMarginProperty);
            set => SetValue(ToggleButtonMarginProperty, value);
        }

        public static readonly DependencyProperty IconSizeProperty =
            DependencyProperty.Register(
                nameof(IconSize),
                typeof(double),
                typeof(RevealableSecretBox),
                new PropertyMetadata(16d));

        public double IconSize
        {
            get => (double)GetValue(IconSizeProperty);
            set => SetValue(IconSizeProperty, value);
        }

        // ===== Styling =====

        public static readonly DependencyProperty ToggleButtonStyleProperty =
            DependencyProperty.Register(
                nameof(ToggleButtonStyle),
                typeof(Style),
                typeof(RevealableSecretBox),
                new PropertyMetadata(null));

        public Style ToggleButtonStyle
        {
            get => (Style)GetValue(ToggleButtonStyleProperty);
            set => SetValue(ToggleButtonStyleProperty, value);
        }

        public static readonly DependencyProperty ToggleToolTipProperty =
            DependencyProperty.Register(
                nameof(ToggleToolTip),
                typeof(object),
                typeof(RevealableSecretBox),
                new PropertyMetadata(null));

        public object ToggleToolTip
        {
            get => GetValue(ToggleToolTipProperty);
            set => SetValue(ToggleToolTipProperty, value);
        }

        // Unified field look
        public static readonly DependencyProperty FieldBackgroundProperty =
            DependencyProperty.Register(
                nameof(FieldBackground),
                typeof(Brush),
                typeof(RevealableSecretBox),
                new PropertyMetadata(Brushes.Transparent));

        public Brush FieldBackground
        {
            get => (Brush)GetValue(FieldBackgroundProperty);
            set => SetValue(FieldBackgroundProperty, value);
        }

        public static readonly DependencyProperty FieldBorderBrushProperty =
            DependencyProperty.Register(
                nameof(FieldBorderBrush),
                typeof(Brush),
                typeof(RevealableSecretBox),
                new PropertyMetadata(Brushes.Transparent));

        public Brush FieldBorderBrush
        {
            get => (Brush)GetValue(FieldBorderBrushProperty);
            set => SetValue(FieldBorderBrushProperty, value);
        }

        public static readonly DependencyProperty FieldBorderThicknessProperty =
            DependencyProperty.Register(
                nameof(FieldBorderThickness),
                typeof(Thickness),
                typeof(RevealableSecretBox),
                new PropertyMetadata(new Thickness(0)));

        public Thickness FieldBorderThickness
        {
            get => (Thickness)GetValue(FieldBorderThicknessProperty);
            set => SetValue(FieldBorderThicknessProperty, value);
        }

        public static readonly DependencyProperty FieldCornerRadiusProperty =
            DependencyProperty.Register(
                nameof(FieldCornerRadius),
                typeof(CornerRadius),
                typeof(RevealableSecretBox),
                new PropertyMetadata(new CornerRadius(0)));

        public CornerRadius FieldCornerRadius
        {
            get => (CornerRadius)GetValue(FieldCornerRadiusProperty);
            set => SetValue(FieldCornerRadiusProperty, value);
        }

        // ===== Icon sources =====

        public static readonly DependencyProperty EyeClosedIconSourceProperty =
            DependencyProperty.Register(
                nameof(EyeClosedIconSource),
                typeof(string),
                typeof(RevealableSecretBox),
                new PropertyMetadata("/Assets/Icons/Eye-Closed-48x48.svg"));

        public string EyeClosedIconSource
        {
            get => (string)GetValue(EyeClosedIconSourceProperty);
            set => SetValue(EyeClosedIconSourceProperty, value);
        }

        public static readonly DependencyProperty EyeOpenIconSourceProperty =
            DependencyProperty.Register(
                nameof(EyeOpenIconSource),
                typeof(string),
                typeof(RevealableSecretBox),
                new PropertyMetadata("/Assets/Icons/Eye-Open-48x48.svg"));

        public string EyeOpenIconSource
        {
            get => (string)GetValue(EyeOpenIconSourceProperty);
            set => SetValue(EyeOpenIconSourceProperty, value);
        }
    }
}
