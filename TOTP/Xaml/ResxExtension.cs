using System;
using System.Windows.Markup;

namespace TOTP.Xaml
{
    [MarkupExtensionReturnType(typeof(string))]
    public class ResxExtension : MarkupExtension
    {
        public string Key { get; set; } = string.Empty;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return TOTP.Resources.UI.ResourceManager.GetString(Key)
                   ?? $"!{Key}!";
        }
    }

}
