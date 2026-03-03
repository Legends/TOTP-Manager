using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Serilog;
using Serilog.Events;
using TOTP.Core.Services;
using TOTP.Core.Services.Interfaces;
using TOTP.Services.Interfaces;
using TOTP.Core.Security.Interfaces;

namespace TOTP.Infrastructure.Services;

public sealed class ClipboardBackgroundService : BackgroundService, IClipboardService
{
    private readonly ILogger<ClipboardBackgroundService> _logger;
    private readonly IAppSettingsDAL _profileStore;

    private string? _lastCopiedText;
    private DateTime? _clearAt;
    private readonly object _lock = new();
    private ILogSwitchService _lss;
    public ClipboardBackgroundService(ILogger<ClipboardBackgroundService> logger, IAppSettingsDAL profileStore, ILogSwitchService lss)
    {
        _lss = lss;
        _logger = logger;
        _profileStore = profileStore;
    }

    public void CopyAndScheduleClear(string text, TimeSpan? duration = null)
    {
        if (string.IsNullOrEmpty(text)) return;

        // 1. Perform the copy on the UI Thread (STA requirement)
        Application.Current.Dispatcher.Invoke(() =>
        {
            Clipboard.SetText(text);
        });

        // 2. Schedule the clear
        lock (_lock)
        {
            _lastCopiedText = text;

            var clearAfter = duration ?? TimeSpan.FromSeconds(30);
            _clearAt = DateTime.UtcNow.Add(clearAfter);
        }

        var level = _lss.GetLevel();
        Log.Write(LogEventLevel.Verbose, $"Current logging level: {level}");
        _logger.LogInformation("Sensitive data copied. Scheduled to clear in {Duration}s", (duration ?? TimeSpan.FromSeconds(30)).TotalSeconds);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Check every second
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            DateTime? clearTime;
            string? expectedText;

            lock (_lock)
            {
                clearTime = _clearAt;
                expectedText = _lastCopiedText;
            }

            if (clearTime.HasValue && DateTime.UtcNow >= clearTime.Value)
            {
                await ClearIfUnchanged(expectedText);

                lock (_lock)
                {
                    _clearAt = null;
                    _lastCopiedText = null;
                }
            }
        }
    }

    private async Task ClearIfUnchanged(string? expectedText)
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            try
            {
                // Professional touch: Only clear if the user hasn't copied something else in the meantime
                if (Clipboard.ContainsText() && Clipboard.GetText() == expectedText)
                {
                    Clipboard.Clear();
                    _logger.LogInformation("Clipboard cleared automatically.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to clear clipboard.");
            }
        });
    }
}


public interface IClipboardService
{
    /// <summary>
    /// Copies text to clipboard and schedules it to be cleared.
    /// </summary>
    void CopyAndScheduleClear(string text, TimeSpan? duration = null);
}