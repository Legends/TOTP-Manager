using TOTP.Interfaces;
using System;
using System.IO;

namespace TOTP.Services;

public class ErrorHandler : IErrorHandler
{
    IMessageService _msgSvc;
    public ErrorHandler(IMessageService msgSvc)
    {
        _msgSvc = msgSvc ?? throw new ArgumentNullException(nameof(msgSvc));
    }
    public void Handle(Exception exception, string userMessage)
    {
        // 1. Show user-friendly message
        _msgSvc.ShowMessage(
            $"{userMessage}\n\nDetails:\n{exception.Message}",
            "Error");

        // 2. Log details (optional)
        LogException(exception);
    }

    private void LogException(Exception ex)
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
