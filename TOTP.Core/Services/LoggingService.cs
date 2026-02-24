using Serilog.Core;
using Serilog.Events;

namespace TOTP.Core.Services
{
    public interface ILogSwitchService
    {
        LoggingLevelSwitch ControlSwitch { get; }
        void SetLevel(LogEventLevel level);
    }

    public class LogSwitchService : ILogSwitchService
    {
        public LoggingLevelSwitch ControlSwitch { get; } = new();

        public void SetLevel(LogEventLevel level)
        {
            ControlSwitch.MinimumLevel = level;
        }
    }
}