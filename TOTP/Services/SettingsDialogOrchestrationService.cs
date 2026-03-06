using System;
using System.Linq;
using System.Threading.Tasks;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Services.Interfaces;
using TOTP.Services.Interfaces;
using TOTP.ViewModels;

namespace TOTP.Services;

public sealed class SettingsDialogOrchestrationService(
    ISettingsService settingsService,
    IAuthorizationService authorizationService,
    ITokenTransferWorkflowService tokenTransferWorkflowService,
    ISettingsAuthorizationWorkflowService settingsAuthorizationWorkflowService,
    ISettingsPersistenceService settingsPersistenceService,
    IMessageService messageService,
    ILogSwitchService logSwitchService) : ISettingsDialogOrchestrationService
{
    public async Task<SettingsViewModel> CreateAndLoadAsync(
        System.Windows.Input.ICommand closeCommand,
        Action saveAction,
        ITokensCollectionContext accountsCollectionContext)
    {
        var loadResult = await settingsService.LoadAsync();
        if (loadResult.IsFailed)
        {
            throw new InvalidOperationException(string.Join("; ", loadResult.Errors.Select(e => e.Message)));
        }

        var settingsTransferWorkflowService = new SettingsTransferWorkflowService(
            tokenTransferWorkflowService,
            accountsCollectionContext);

        var vm = new SettingsViewModel(
            settingsService,
            authorizationService,
            settingsAuthorizationWorkflowService,
            settingsPersistenceService,
            settingsTransferWorkflowService,
            messageService,
            logSwitchService,
            closeCommand,
            saveAction);

        await vm.LoadAsync();
        return vm;
    }
}
