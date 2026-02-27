using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TOTP.Core.Services.Interfaces;

public interface ILogSwitchService
{
    /// <summary>
    /// Gets a value indicating whether a command-line interface (CLI) override is currently active.
    /// </summary>
    bool IsCliOverrideActive { get; set; }

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
