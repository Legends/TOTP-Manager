using System;
using System.IO;
using TOTP.Enums;
using TOTP.Helper;
using TOTP.Interfaces;

namespace TOTP.Services;

public class ErrorHandler : IErrorHandler
{
    private readonly IMessageService _msgSvc;

    public ErrorHandler(IMessageService msgSvc)
    {
        ArgumentNullException.ThrowIfNull(msgSvc, nameof(msgSvc));
        _msgSvc = msgSvc;
    }

    public void Handle(Exception exception, string userMessage)
    {
        // 1. Show user-friendly message
        _msgSvc.ShowMessage(
            $"{userMessage}\n\nDetails:\n{exception.Message}",
            CaptionType.Error, StringsConstants.ImgError
        );

        // 2. Log details (optional)
        LogException(exception);
    }

    private static void LogException(Exception ex)
    {
        try
        {
            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
            File.AppendAllText(logPath, $"[{DateTime.Now}] {ex}\n\n");
        }
        catch
        {
            // Swallow logging exceptions
        }
    }
}