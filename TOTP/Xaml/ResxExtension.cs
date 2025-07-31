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

            void Update() => targetObject.SetValue(targetProperty, TOTP.Resources.UI.ResourceManager.GetString(Key) ?? $"!{Key}!");

            LocalizationService.LanguageChanged += Update;

            switch (targetObject)
            {
                case FrameworkElement fe:
                    fe.Unloaded += (_, _) => LocalizationService.LanguageChanged -= Update;
                    break;
                case FrameworkContentElement fce:
                    fce.Unloaded += (_, _) => LocalizationService.LanguageChanged -= Update;
                    break;
            }
        }

        return value;
    }
}