using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace TOTP.Avalonia.Shared.Controls;

public sealed class ValidationMessage : TemplatedControl
{
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<ValidationMessage, string>(nameof(Text), string.Empty);

    public static readonly StyledProperty<ValidationSeverity> SeverityProperty =
        AvaloniaProperty.Register<ValidationMessage, ValidationSeverity>(
            nameof(Severity),
            ValidationSeverity.Information);

    static ValidationMessage()
    {
        TextProperty.Changed.AddClassHandler<ValidationMessage>(
            static (control, _) => control.UpdateVisualState());
        SeverityProperty.Changed.AddClassHandler<ValidationMessage>(
            static (control, _) => control.UpdateVisualState());
    }

    public ValidationMessage()
    {
        UpdateVisualState();
    }

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value ?? string.Empty);
    }

    public ValidationSeverity Severity
    {
        get => GetValue(SeverityProperty);
        set => SetValue(SeverityProperty, value);
    }

    private void UpdateVisualState()
    {
        IsVisible = !string.IsNullOrWhiteSpace(Text);
        PseudoClasses.Set(":information", Severity == ValidationSeverity.Information);
        PseudoClasses.Set(":warning", Severity == ValidationSeverity.Warning);
        PseudoClasses.Set(":error", Severity == ValidationSeverity.Error);
    }
}
