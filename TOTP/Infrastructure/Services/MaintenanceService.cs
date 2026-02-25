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

public sealed class BackupService : IHostedService
{
    private readonly IAccountsManager _accountsManager;
    private readonly ILogger<BackupService> _logger;

    public BackupService(IAccountsManager accountsManager, ILogger<BackupService> logger)
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
            await _accountsManager.BackupAccountsStorageFileAsync();
            _logger.LogInformation("Shutdown backup completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Shutdown backup failed.");
        }
    }
}
