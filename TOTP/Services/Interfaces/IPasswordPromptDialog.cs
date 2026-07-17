namespace TOTP.Services.Interfaces;

public interface IPasswordPromptDialog
{
    object? DataContext { get; set; }
    bool? ShowDialog();
}
