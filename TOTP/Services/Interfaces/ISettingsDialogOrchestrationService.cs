using System;
using System.Threading.Tasks;
using System.Windows.Input;
using TOTP.ViewModels;

namespace TOTP.Services.Interfaces;

public interface ISettingsDialogOrchestrationService
{
    Task<SettingsViewModel> CreateAndLoadAsync(
        ICommand closeCommand,
        Action saveAction,
        IAccountsCollectionContext accountsCollectionContext);
}
