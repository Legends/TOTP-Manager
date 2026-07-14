using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TOTP.Core.Security.Models;
using TOTP.Views.Interfaces;

namespace TOTP.Presentation.Services.Interfaces;

public interface IMainViewSessionController
{
    AppSessionLockState SessionState { get; }
    bool IsUnlocked { get; }

    event EventHandler<AppSessionLockState>? SessionStateChanged;

    ICommand WindowStateChangedCommand { get; }
    ICommand DetachWindowCommand { get; }

    void ConfigureCallbacks(Func<Task> onUnlockedAsync, Action onLocked);
    Task InitializeAsync(IMainWindow? mainWindow);
    Task InitializeAsync(IMainWindow? mainWindow, CancellationToken ct);
    Task<AuthorizationResult> TryUnlockOnStartupAsync(CancellationToken ct);
    void AttachWindow(IMainWindow? window);
    void Lock();
}
