namespace TOTP.Services.Interfaces
{
    public interface IQrScannerDialogService
    {
        /// <summary>
        /// Opens the QR scanner modal dialog and returns the decoded text if successful; otherwise null.
        /// </summary>
        string? ScanQrCode(System.Windows.Window owner);
    }
}
