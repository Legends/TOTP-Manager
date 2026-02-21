using System;
using System.Threading.Tasks;
using System.Windows.Input;
using TOTP.Services.Interfaces;
using TOTP.Security.Models;

namespace TOTP.Security.Interfaces;

public interface IMainViewSessionController
{
    AppSessionState SessionState { get; }
    bool IsUnlocked { get; }

    event EventHandler<AppSessionState>? SessionStateChanged;

    ICommand WindowStateChangedCommand { get; }
    ICommand DetachWindowCommand { get; }

    void ConfigureCallbacks(Func<Task> onUnlockedAsync, Action onLocked);
    Task InitializeAsync(IMainWindow? mainWindow);
    void AttachWindow(IMainWindow? window);
    void Lock();
}
