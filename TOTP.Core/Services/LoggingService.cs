using Serilog.Core;
using Serilog.Events;

namespace TOTP.Core.Services
{
    public interface ILogSwitchService
    {
        /// <summary>
        /// Access the underlying Serilog level switch.
        /// </summary>
        LoggingLevelSwitch ControlSwitch { get; }

        /// <summary>
        /// Updates the minimum logging level globally.
        /// </summary>
        void SetLevel(LogEventLevel level);

        /// <summary>
        /// Retrieves the current minimum logging level.
        /// </summary>
        LogEventLevel GetLevel();
    }

    public class LogSwitchService : ILogSwitchService
    {
        // The actual "Source of Truth"
        private static readonly LoggingLevelSwitch _masterSwitch = new(LogEventLevel.Information);

        // Provide a static way to get it (Fixes CS0117)
        public static LoggingLevelSwitch SharedSwitch => _masterSwitch;

        // Interface implementation (Instance members)
        public LoggingLevelSwitch ControlSwitch => _masterSwitch;

        public void SetLevel(LogEventLevel level) => _masterSwitch.MinimumLevel = level;
        public LogEventLevel GetLevel() => _masterSwitch.MinimumLevel;
    }
}