using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace TOTP.AttachedProperties
{
    /// <summary>
    /// Attached properties to implement a lightweight slide-in flyout.
    /// Use with a Grid (host) + TranslateTransform animation in XAML.
    /// Provides:
    /// - IsOpen (bool)
    /// - ClosedX (double): off-screen X translate when closed (e.g. 420)
    /// - Duration (Duration): animation duration
    /// - CloseOnEsc (bool): closes when ESC is pressed (PreviewKeyDown)
    /// - CloseCommand (ICommand): optional command to invoke on close
    /// - IsModal (bool): if true, overlay should block background input (XAML handles this)
    /// </summary>
    public static class Flyout
    {
        // -------------------- IsOpen --------------------

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.RegisterAttached(
                "IsOpen",
                typeof(bool),
                typeof(Flyout),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static void SetIsOpen(DependencyObject element, bool value) => element.SetValue(IsOpenProperty, value);
        public static bool GetIsOpen(DependencyObject element) => (bool)element.GetValue(IsOpenProperty);

        // -------------------- ClosedX --------------------

        public static readonly DependencyProperty ClosedXProperty =
            DependencyProperty.RegisterAttached(
                "ClosedX",
                typeof(double),
                typeof(Flyout),
                new FrameworkPropertyMetadata(420d));

        public static void SetClosedX(DependencyObject element, double value) => element.SetValue(ClosedXProperty, value);
        public static double GetClosedX(DependencyObject element) => (double)element.GetValue(ClosedXProperty);

        // -------------------- Duration --------------------

        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.RegisterAttached(
                "Duration",
                typeof(Duration),
                typeof(Flyout),
                new FrameworkPropertyMetadata(new Duration(TimeSpan.FromMilliseconds(180))));

        public static void SetDuration(DependencyObject element, Duration value) => element.SetValue(DurationProperty, value);
        public static Duration GetDuration(DependencyObject element) => (Duration)element.GetValue(DurationProperty);

        // -------------------- CloseOnEsc --------------------

        public static readonly DependencyProperty CloseOnEscProperty =
            DependencyProperty.RegisterAttached(
                "CloseOnEsc",
                typeof(bool),
                typeof(Flyout),
                new FrameworkPropertyMetadata(true, OnCloseOnEscChanged));

        public static void SetCloseOnEsc(DependencyObject element, bool value) => element.SetValue(CloseOnEscProperty, value);
        public static bool GetCloseOnEsc(DependencyObject element) => (bool)element.GetValue(CloseOnEscProperty);

        private static void OnCloseOnEscChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not UIElement ui) return;

            var enable = (bool)e.NewValue;

            if (enable)
                ui.PreviewKeyDown += UiOnPreviewKeyDown;
            else
                ui.PreviewKeyDown -= UiOnPreviewKeyDown;
        }

        // -------------------- CloseCommand --------------------

        public static readonly DependencyProperty CloseCommandProperty =
            DependencyProperty.RegisterAttached(
                "CloseCommand",
                typeof(ICommand),
                typeof(Flyout),
                new FrameworkPropertyMetadata(null));

        public static void SetCloseCommand(DependencyObject element, ICommand? value) => element.SetValue(CloseCommandProperty, value);
        public static ICommand? GetCloseCommand(DependencyObject element) => (ICommand?)element.GetValue(CloseCommandProperty);

        // -------------------- IsModal (optional, XAML uses it) --------------------

        public static readonly DependencyProperty IsModalProperty =
            DependencyProperty.RegisterAttached(
                "IsModal",
                typeof(bool),
                typeof(Flyout),
                new FrameworkPropertyMetadata(true));

        public static void SetIsModal(DependencyObject element, bool value) => element.SetValue(IsModalProperty, value);
        public static bool GetIsModal(DependencyObject element) => (bool)element.GetValue(IsModalProperty);

        // -------------------- Helpers --------------------

        private static void UiOnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape) return;

            if (sender is not DependencyObject d) return;

            // Close flyout
            if (GetIsOpen(d))
            {
                SetIsOpen(d, false);

                // Optional command
                var cmd = GetCloseCommand(d);
                if (cmd?.CanExecute(null) == true)
                    cmd.Execute(null);

                e.Handled = true;
            }
        }
    }
}
