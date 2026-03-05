namespace TOTP.Services.Interfaces;

public interface IPasswordPromptDialogFactory
{
    IPasswordPromptDialog CreateExportPasswordPromptDialog();
    IPasswordPromptDialog CreatePasswordPromptDialog();
}
