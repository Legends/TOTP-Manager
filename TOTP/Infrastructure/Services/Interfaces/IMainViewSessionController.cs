using System;
using System.Threading.Tasks;
using System.Windows.Input;
using TOTP.Security.Models;
using TOTP.Views.Interfaces;

namespace TOTP.Security.Interfaces;

public interface IMainViewSessionController
{
    AppSessionLockState SessionState { get; }
    bool IsUnlocked { get; }

    event EventHandler<AppSessionLockState>? SessionStateChanged;

    ICommand WindowStateChangedCommand { get; }
    ICommand DetachWindowCommand { get; }

    void ConfigureCallbacks(Func<Task> onUnlockedAsync, Action onLocked);
    Task InitializeAsync(IMainWindow? mainWindow);
    void AttachWindow(IMainWindow? window);
    void Lock();
}
