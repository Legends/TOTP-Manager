using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;

namespace TOTP.Infrastructure.Services;

public sealed class SettingsService : ISettingsService, IDisposable
{
    private readonly IAppSettingsDAL _store;
    private readonly ILogger<SettingsService> _logger;

    // Semaphore prevents race conditions during Load/Save 
    // while allowing async operations.
    private readonly SemaphoreSlim _lock = new(1, 1);

    private IAppSettings _current = new AppSettings();

    /// <summary>
    /// Thread-safe access to the current settings.
    /// </summary>
    public IAppSettings Current
    {
        get
        {
            // Simple return is thread-safe for reference types in .NET,
            // but the Semaphore ensures the object isn't being replaced mid-read.
            return _current;
        }
        private set => _current = value;
    }

    public SettingsService(IAppSettingsDAL store, ILogger<SettingsService> logger)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private bool _isLoaded;

    public async Task<IAppSettings> LoadAsync()
    {
        if (_isLoaded) return Current; // Immediate return if already in memory

        await _lock.WaitAsync();
        try
        {
            if (_isLoaded) return Current; // Double-check lock pattern

            _logger.LogDebug("Loading settings from store for the first time...");
            var settings = await _store.LoadAsync();

            if (settings != null)
            {
                Current = settings;
            }

            _isLoaded = true; // Mark as loaded
            return Current;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SaveAsync()
    {
        await _lock.WaitAsync();
        try
        {
            _logger.LogDebug("Persisting global settings...");
            await _store.SaveAsync(Current);
            _logger.LogInformation("Global settings saved successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist global settings.");
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Dispose()
    {
        _lock.Dispose();
    }
}