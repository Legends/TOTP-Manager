namespace TOTP.Core.Events;

public class AddNewPromptArgs : EventArgs
{
    public bool Success { get; set; }
    public string? Platform { get; set; }
    public string? Secret { get; set; }
    public string? Account { get; set; }
}
