namespace TOTP.Services.Interfaces
{
    public interface IFileDialogService
    {
        string? ShowSaveFileDialog(string filter, string defaultExt, string? suggestedFileName = null);
        string? ShowOpenFileDialog(string filter, string defaultExt);
    }

}
