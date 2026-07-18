using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace TOTP.Avalonia.Shared.Controls;

public sealed class QrPreview : TemplatedControl
{
    public static readonly StyledProperty<IImage?> SourceProperty =
        AvaloniaProperty.Register<QrPreview, IImage?>(nameof(Source));

    public static readonly StyledProperty<string> DescriptionProperty =
        AvaloniaProperty.Register<QrPreview, string>(
            nameof(Description),
            "QR code containing account credentials");

    public static readonly StyledProperty<string> PrivacyNoticeProperty =
        AvaloniaProperty.Register<QrPreview, string>(
            nameof(PrivacyNotice),
            "This QR code contains the account secret. Keep it private.");

    static QrPreview()
    {
        SourceProperty.Changed.AddClassHandler<QrPreview>(
            static (control, _) => control.UpdateVisibility());
    }

    public QrPreview()
    {
        UpdateVisibility();
    }

    public IImage? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value ?? string.Empty);
    }

    public string PrivacyNotice
    {
        get => GetValue(PrivacyNoticeProperty);
        set => SetValue(PrivacyNoticeProperty, value ?? string.Empty);
    }

    private void UpdateVisibility() => IsVisible = Source is not null;
}
