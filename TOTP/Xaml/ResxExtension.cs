using System;
using System.Windows;
using System.Windows.Markup;
using TOTP.Services;

namespace TOTP.Xaml;

[MarkupExtensionReturnType(typeof(string))]
public class ResxExtension : MarkupExtension
{
    public string Key { get; set; } = string.Empty;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var value = TOTP.Resources.UI.ResourceManager.GetString(Key) ?? $"!{Key}!";

        if (serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget targetService &&
            targetService.TargetObject is DependencyObject targetObject &&
            targetService.TargetProperty is DependencyProperty targetProperty)
        {

            void Update()
            {
                var dispatcher = targetObject.Dispatcher;
                if (dispatcher.HasShutdownStarted || dispatcher.HasShutdownFinished)
                {
                    return;
                }

                void ApplyValue()
                {
                    targetObject.SetValue(targetProperty, TOTP.Resources.UI.ResourceManager.GetString(Key) ?? $"!{Key}!");
                }

                if (dispatcher.CheckAccess())
                {
                    ApplyValue();
                    return;
                }

                _ = dispatcher.BeginInvoke(ApplyValue);
            }

            LocalizationEventHub.LanguageChanged += Update;

            switch (targetObject)
            {
                case FrameworkElement fe:
                    fe.Unloaded += (_, _) => LocalizationEventHub.LanguageChanged -= Update;
                    break;
                case FrameworkContentElement fce:
                    fce.Unloaded += (_, _) => LocalizationEventHub.LanguageChanged -= Update;
                    break;
            }
        }

        return value;
    }
}
