using TOTP.Interfaces;

namespace TOTP.Services
{
    using Microsoft.Win32;

    public sealed class FileDialogService : IFileDialogService
    {
        public string? ShowSaveFileDialog(string filter, string defaultExt, string? suggestedFileName = null)
        {
            var dlg = new SaveFileDialog
            {
                Filter = filter,
                DefaultExt = defaultExt,
                FileName = suggestedFileName ?? string.Empty,
                AddExtension = true,
                OverwritePrompt = true
            };

            return dlg.ShowDialog() == true ? dlg.FileName : null;
        }
    }

}
