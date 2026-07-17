using FluentResults;
using TOTP.Core.Services.Models;

namespace TOTP.Core.Services.Interfaces;

public interface IPlatformClipboard
{
    ClipboardCapabilities Capabilities { get; }
    Result<ClipboardWriteReceipt> SetText(string text);
    Result<bool> ClearIfUnchanged(ClipboardWriteReceipt receipt);
}
