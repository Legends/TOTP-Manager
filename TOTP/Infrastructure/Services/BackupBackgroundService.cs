using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TOTP.Core.Services.Interfaces;

namespace TOTP.Infrastructure.Services;

public sealed class BackupBackgroundService : IHostedService
{
    private readonly IOtpManager _accountsManager;
    private readonly ILogger<BackupBackgroundService> _logger;

    public BackupBackgroundService(IOtpManager accountsManager, ILogger<BackupBackgroundService> logger)
    {
        _accountsManager = accountsManager;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting shutdown backup...");
            await _accountsManager.BackupOtpEntriesStorageFileAsync();
            _logger.LogInformation("Shutdown backup completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Shutdown backup failed.");
        }
    }
}
