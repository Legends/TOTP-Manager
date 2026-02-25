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

public sealed class MaintenanceService : IHostedService
{
    private readonly IAccountsManager _accountsManager;
    private readonly ILogger<MaintenanceService> _logger;

    public MaintenanceService(IAccountsManager accountsManager, ILogger<MaintenanceService> logger)
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
