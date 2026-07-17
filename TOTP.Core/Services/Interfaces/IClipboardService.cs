using FluentResults;
using TOTP.Core.Services.Models;

namespace TOTP.Core.Services.Interfaces;

public interface IClipboardService
{
    ClipboardCapabilities Capabilities { get; }
    Result CopyAndScheduleClear(string text, TimeSpan? duration = null);
    Result SetText(string text);
}
