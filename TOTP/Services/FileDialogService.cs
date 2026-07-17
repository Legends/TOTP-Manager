using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using TOTP.Core.Services.Interfaces;
using TOTP.Services.Interfaces;

namespace TOTP.Services;

public sealed class FileDialogService : IFileDialogService
{
    public string? ShowSaveFileDialog(SaveFileDialogRequest request)
    {
        var dialog = new SaveFileDialog
        {
            Filter = ToWpfFilter(request.Filters),
            DefaultExt = request.DefaultExtension,
            FileName = request.SuggestedFileName ?? string.Empty,
            AddExtension = true,
            OverwritePrompt = true
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? ShowOpenFileDialog(OpenFileDialogRequest request)
    {
        var dialog = new OpenFileDialog
        {
            Filter = ToWpfFilter(request.Filters),
            DefaultExt = request.DefaultExtension,
            CheckFileExists = true,
            Multiselect = false
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    private static string ToWpfFilter(IEnumerable<FileDialogFilter> filters) =>
        string.Join('|', filters.Select(filter =>
            $"{filter.DisplayName}|{string.Join(';', filter.Patterns)}"));
}
