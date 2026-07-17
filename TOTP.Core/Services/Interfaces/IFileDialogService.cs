namespace TOTP.Core.Services.Interfaces;

public interface IFileDialogService
{
    string? ShowSaveFileDialog(SaveFileDialogRequest request);
    string? ShowOpenFileDialog(OpenFileDialogRequest request);
}

public sealed record FileDialogFilter(string DisplayName, params string[] Patterns);

public sealed record SaveFileDialogRequest(
    IReadOnlyList<FileDialogFilter> Filters,
    string DefaultExtension,
    string? SuggestedFileName = null);

public sealed record OpenFileDialogRequest(
    IReadOnlyList<FileDialogFilter> Filters,
    string DefaultExtension);
