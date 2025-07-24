using System.Windows;
using TOTP.Interfaces;

namespace TOTP.Services;

public class ClipboardService : IClipboardService
{
    public void SetText(string text)
    {
        Clipboard.SetText(text, TextDataFormat.UnicodeText);
    }
}