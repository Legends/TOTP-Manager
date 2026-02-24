using Serilog.Core;
using Serilog.Events;

namespace TOTP.Core.Services
{
    public interface ILoggingService
    {
        LoggingLevelSwitch ControlSwitch { get; }
        void SetLevel(LogEventLevel level);
    }

    public class LoggingService : ILoggingService
    {
        public LoggingLevelSwitch ControlSwitch { get; } = new();

        public void SetLevel(LogEventLevel level)
        {
            ControlSwitch.MinimumLevel = level;
        }
    }
}