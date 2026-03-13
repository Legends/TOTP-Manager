using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace TOTP.AutoUpdate;

internal static class AutoUpdateStepAnimation
{
    private static readonly Duration AnimationDuration = TimeSpan.FromMilliseconds(180);

    public static void Attach(UserControl control)
    {
        ArgumentNullException.ThrowIfNull(control);

        control.RenderTransform = CreateTransform();
        control.RenderTransformOrigin = new Point(0.5, 0.5);
        control.IsVisibleChanged += (_, args) =>
        {
            if (args.NewValue is true)
            {
                BeginEntrance(control);
            }
        };
    }

    private static void BeginEntrance(UIElement element)
    {
        element.Opacity = 0;
        if (element.RenderTransform is not TranslateTransform translateTransform)
        {
            translateTransform = CreateTransform();
            element.RenderTransform = translateTransform;
        }

        translateTransform.Y = 10;

        var storyboard = new Storyboard();

        var opacityAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = AnimationDuration,
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var slideAnimation = new DoubleAnimation
        {
            From = 10,
            To = 0,
            Duration = AnimationDuration,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        Storyboard.SetTarget(opacityAnimation, element);
        Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(UIElement.OpacityProperty));
        Storyboard.SetTarget(slideAnimation, element);
        Storyboard.SetTargetProperty(slideAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));

        storyboard.Children.Add(opacityAnimation);
        storyboard.Children.Add(slideAnimation);
        storyboard.Begin();
    }

    private static TranslateTransform CreateTransform() => new();
}
