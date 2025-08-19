using System;
using System.IO;
using System.Windows.Forms;
using TOTP.Core.Interfaces;

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
        try
        {
            _msgSvc.ShowErrorMessage($"{userMessage}\n\nDetails:\n{exception.Message}");
        }
        finally
        {
            LogException(exception);
        }
    }

    private static void LogException(Exception ex)
    {
        try
        {
            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
            File.AppendAllText(logPath, $"[{DateTime.Now}] {ex}\n\n");
        }
        catch (Exception lex)
        {
            MessageBox.Show(lex.Message);
        }
    }
}