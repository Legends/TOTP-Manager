using System;
using System.IO;
using System.Windows;
using TOTP.Services.Interfaces;

namespace TOTP.Services;

public class ErrorHandler : IErrorHandler
{
    private readonly IMessageService _messageService;

    public ErrorHandler(IMessageService msgSvc)
    {
        ArgumentNullException.ThrowIfNull(msgSvc, nameof(msgSvc));
        _messageService = msgSvc;
    }

    public void Handle(Exception exception, string userMessage)
    {
        try
        {
            _messageService.ShowError($"{userMessage}\n\n\n{exception.Message}");
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