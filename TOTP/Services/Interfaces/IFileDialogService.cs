namespace TOTP.Services.Interfaces
{
    public interface IFileDialogService
    {
        string? ShowSaveFileDialog(string filter, string defaultExt, string? suggestedFileName = null);
    }

}
