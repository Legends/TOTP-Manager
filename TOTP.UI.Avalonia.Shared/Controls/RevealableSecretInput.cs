using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace TOTP.Avalonia.Shared.Controls;

public sealed class RevealableSecretInput : TemplatedControl
{
    private const char MaskCharacter = '●';
    private Button? _revealButton;
    private TextBox? _textBox;

    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<RevealableSecretInput, string>(
            nameof(Text),
            string.Empty,
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<string> PlaceholderTextProperty =
        AvaloniaProperty.Register<RevealableSecretInput, string>(
            nameof(PlaceholderText),
            "Secret");

    public static readonly StyledProperty<string> AccessibleNameProperty =
        AvaloniaProperty.Register<RevealableSecretInput, string>(
            nameof(AccessibleName),
            "Secret");

    public static readonly StyledProperty<string> HelpTextProperty =
        AvaloniaProperty.Register<RevealableSecretInput, string>(
            nameof(HelpText),
            string.Empty);

    public static readonly StyledProperty<bool> IsRequiredProperty =
        AvaloniaProperty.Register<RevealableSecretInput, bool>(nameof(IsRequired));

    public static readonly StyledProperty<bool> IsRevealedProperty =
        AvaloniaProperty.Register<RevealableSecretInput, bool>(nameof(IsRevealed));

    static RevealableSecretInput()
    {
        TextProperty.Changed.AddClassHandler<RevealableSecretInput>(
            static (control, _) => control.OnTextChanged());
        IsRevealedProperty.Changed.AddClassHandler<RevealableSecretInput>(
            static (control, _) => control.UpdateDisclosureState());
    }

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value ?? string.Empty);
    }

    public string PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value ?? string.Empty);
    }

    public string AccessibleName
    {
        get => GetValue(AccessibleNameProperty);
        set => SetValue(AccessibleNameProperty, value ?? string.Empty);
    }

    public string HelpText
    {
        get => GetValue(HelpTextProperty);
        set => SetValue(HelpTextProperty, value ?? string.Empty);
    }

    public bool IsRequired
    {
        get => GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }

    public bool IsRevealed
    {
        get => GetValue(IsRevealedProperty);
        private set => SetValue(IsRevealedProperty, value);
    }

    public void Conceal() => IsRevealed = false;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        DetachRevealHandlers();
        base.OnApplyTemplate(e);

        _textBox = e.NameScope.Find<TextBox>("PART_Input");
        _revealButton = e.NameScope.Find<Button>("PART_RevealButton");
        AttachRevealHandlers();
        UpdateDisclosureState();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Conceal();
        DetachRevealHandlers();
        _textBox = null;
        base.OnDetachedFromVisualTree(e);
    }

    private void AttachRevealHandlers()
    {
        if (_revealButton is null) return;

        _revealButton.AddHandler(
            InputElement.PointerPressedEvent,
            OnRevealPointerPressed,
            RoutingStrategies.Tunnel,
            handledEventsToo: true);
        _revealButton.AddHandler(
            InputElement.PointerReleasedEvent,
            OnRevealPointerReleased,
            RoutingStrategies.Tunnel,
            handledEventsToo: true);
        _revealButton.PointerCaptureLost += OnRevealPointerCaptureLost;
        _revealButton.AddHandler(
            InputElement.KeyDownEvent,
            OnRevealKeyDown,
            RoutingStrategies.Tunnel,
            handledEventsToo: true);
        _revealButton.AddHandler(
            InputElement.KeyUpEvent,
            OnRevealKeyUp,
            RoutingStrategies.Tunnel,
            handledEventsToo: true);
        _revealButton.LostFocus += OnRevealLostFocus;
    }

    private void DetachRevealHandlers()
    {
        if (_revealButton is null) return;

        _revealButton.RemoveHandler(InputElement.PointerPressedEvent, OnRevealPointerPressed);
        _revealButton.RemoveHandler(InputElement.PointerReleasedEvent, OnRevealPointerReleased);
        _revealButton.PointerCaptureLost -= OnRevealPointerCaptureLost;
        _revealButton.RemoveHandler(InputElement.KeyDownEvent, OnRevealKeyDown);
        _revealButton.RemoveHandler(InputElement.KeyUpEvent, OnRevealKeyUp);
        _revealButton.LostFocus -= OnRevealLostFocus;
        _revealButton = null;
    }

    private void OnRevealPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        IsRevealed = true;
    }

    private void OnRevealPointerReleased(object? sender, PointerReleasedEventArgs e) => Conceal();

    private void OnRevealPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e) => Conceal();

    private void OnRevealKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is not (Key.Space or Key.Enter)) return;

        IsRevealed = true;
        e.Handled = true;
    }

    private void OnRevealKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key is not (Key.Space or Key.Enter)) return;

        Conceal();
        e.Handled = true;
    }

    private void OnRevealLostFocus(object? sender, RoutedEventArgs e) => Conceal();

    private void OnTextChanged()
    {
        if (string.IsNullOrEmpty(Text)) Conceal();
    }

    private void UpdateDisclosureState()
    {
        PseudoClasses.Set(":revealed", IsRevealed);
        if (_textBox is not null)
        {
            _textBox.PasswordChar = IsRevealed ? '\0' : MaskCharacter;
        }
    }
}
