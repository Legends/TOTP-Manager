namespace TOTP.Interfaces;

public interface IDialogService
{
    /// <summary>
    ///     Shows a Platform/Secret entry dialog and returns the result.
    /// </summary>
    /// <returns>
    ///     Tuple (success, key, value), where 'success' is true if OK was pressed, false if canceled.
    /// </returns>
    (bool success, string? key, string? value) ShowKeyValueDialog();

    (bool success, string? key, string? value) ShowKeyValueDialog(string? initialKey = null,
        string? initialValue = null);
}