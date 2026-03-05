using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace TOTP.Commands;

internal static class CommandExceptionLogger
{
    private static ILoggerFactory _loggerFactory = NullLoggerFactory.Instance;

    public static void Initialize(ILoggerFactory? loggerFactory)
    {
        _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
    }

    public static void LogUnhandled(string commandName, Exception ex)
    {
        var logger = _loggerFactory.CreateLogger(commandName);
        logger.LogError(ex, "Unhandled exception in command {CommandName}", commandName);
    }
}

