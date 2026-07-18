using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using TOTP.Core.Services.Interfaces;

namespace TOTP.Avalonia.Shared.Controls;

public sealed class NotificationBanner : TemplatedControl
{
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<NotificationBanner, string>(nameof(Text), string.Empty);

    public static readonly StyledProperty<NotificationSeverity> SeverityProperty =
        AvaloniaProperty.Register<NotificationBanner, NotificationSeverity>(
            nameof(Severity),
            NotificationSeverity.Information);

    public static readonly StyledProperty<AutomationLiveSetting> LiveSettingProperty =
        AvaloniaProperty.Register<NotificationBanner, AutomationLiveSetting>(
            nameof(LiveSetting),
            AutomationLiveSetting.Polite);

    static NotificationBanner()
    {
        TextProperty.Changed.AddClassHandler<NotificationBanner>(
            static (control, _) => control.UpdateVisualState());
        SeverityProperty.Changed.AddClassHandler<NotificationBanner>(
            static (control, _) => control.UpdateVisualState());
    }

    public NotificationBanner()
    {
        UpdateVisualState();
    }

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value ?? string.Empty);
    }

    public NotificationSeverity Severity
    {
        get => GetValue(SeverityProperty);
        set => SetValue(SeverityProperty, value);
    }

    public AutomationLiveSetting LiveSetting
    {
        get => GetValue(LiveSettingProperty);
        private set => SetValue(LiveSettingProperty, value);
    }

    private void UpdateVisualState()
    {
        IsVisible = !string.IsNullOrWhiteSpace(Text);
        LiveSetting = Severity == NotificationSeverity.Error
            ? AutomationLiveSetting.Assertive
            : AutomationLiveSetting.Polite;
        PseudoClasses.Set(":information", Severity == NotificationSeverity.Information);
        PseudoClasses.Set(":success", Severity == NotificationSeverity.Success);
        PseudoClasses.Set(":warning", Severity == NotificationSeverity.Warning);
        PseudoClasses.Set(":error", Severity == NotificationSeverity.Error);
    }
}
