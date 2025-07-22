using Github2FA.Interfaces;
using System.Windows;

namespace Github2FA.Services
{
    public class ClipboardService : IClipboardService
    {
        public void SetText(string text)
        {
            Clipboard.SetText(text, TextDataFormat.UnicodeText);
        }
    }
}
