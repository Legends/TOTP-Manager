using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TOTP.Commands;
using TOTP.Core.Enums;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Security.Interfaces;
using TOTP.Services.Interfaces;
using TOTP.Views.Interfaces;

namespace TOTP.Security;

public sealed class MainViewSessionController : IMainViewSessionController
{
    private readonly IAuthorizationService _authorization;
    private readonly IInputActivityMonitor _inputActivityMonitor;
    private readonly ISettingsService _settingsService;
    private readonly ILogger<MainViewSessionController> _logger;

    private Func<Task>? _onUnlockedAsync;
    private Action? _onLocked;
    private IMainWindow? _attachedWindow;

    public AppSessionLockState SessionState { get; private set; } = AppSessionLockState.Locked;
    public bool IsUnlocked => SessionState == AppSessionLockState.Unlocked;

    public event EventHandler<AppSessionLockState>? SessionStateChanged;

    public ICommand WindowStateChangedCommand { get; }
    public ICommand DetachWindowCommand { get; }

    public MainViewSessionController(
        IAuthorizationService authorization,
        IInputActivityMonitor inputActivityMonitor,
        ISettingsService settingsService,
        ILogger<MainViewSessionController> logger)
    {
        _logger = logger;
        _authorization = authorization;
        _inputActivityMonitor = inputActivityMonitor;
        _settingsService = settingsService;

        WindowStateChangedCommand = new RelayCommand<WindowState>(OnWindowStateChanged, CanHandleWindowStateChange);
        DetachWindowCommand = new RelayCommand(DetachWindow, CanDetachWindow);

        _authorization.State.Changed += AuthorizationState_Changed;
    }

    public void ConfigureCallbacks(Func<Task> onUnlockedAsync, Action onLocked)
    {
        _onUnlockedAsync = onUnlockedAsync;
        _onLocked = onLocked;
    }

    public async Task InitializeAsync(IMainWindow? mainWindow)
    {
        try
        {
            AttachWindow(mainWindow);
            SetSessionState(AppSessionLockState.Unlocking);

            await _authorization.InitializeAsync();

            var startupUnlockResult = await _authorization.TryUnlockOnStartupAsync();
            if (!IsUnlocked && startupUnlockResult != AuthorizationResult.Success)
            {
                SetSessionState(AppSessionLockState.Locked);
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Critical failure during session initialization.");
            SetSessionState(AppSessionLockState.Locked);
        }
    }

    public void AttachWindow(IMainWindow? window)
    {
        if (window == null) return;

        try
        {
            _attachedWindow = window;
            _inputActivityMonitor.Attach(window);
            RaiseCommandStates();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to attach window to activity monitor.");
        }
    }

    private bool CanHandleWindowStateChange(WindowState state)
    {
        return _attachedWindow != null &&
               state == WindowState.Minimized &&
               _settingsService.Current.LockOnMinimize;
    }

    private bool CanDetachWindow() => _attachedWindow != null;

    private void DetachWindow()
    {
        try
        {
            _inputActivityMonitor.Detach();
            _attachedWindow = null;
            RaiseCommandStates();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error while detaching window.");
        }
    }

    public void Lock()
    {
        SetSessionState(AppSessionLockState.Locked);

        try
        {
            _authorization.Lock();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while calling Lock on AuthorizationService.");
        }
    }

    private void OnWindowStateChanged(WindowState state)
    {
        if (state == WindowState.Minimized && _settingsService.Current.LockOnMinimize)
            Lock();
    }

    private async void AuthorizationState_Changed(object? sender, EventArgs e)
    {
        try
        {
            SetSessionState(_authorization.State.IsUnlocked ? AppSessionLockState.Unlocked : AppSessionLockState.Locked);

            if (IsUnlocked)
            {
                if (_attachedWindow != null)
                {
                    _inputActivityMonitor.Attach(_attachedWindow);
                }

                if (_onUnlockedAsync != null)
                {
                    await _onUnlockedAsync();
                }
            }
            else
            {
                _onLocked?.Invoke();
                _inputActivityMonitor.Detach();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during AuthorizationState change.");

            if (IsUnlocked)
            {
                SetSessionState(AppSessionLockState.Locked);
                _onLocked?.Invoke();
            }
        }
    }

    /// <summary>
    /// Sets the state of the app to one of these:
    /// </summary>
    /// <param name="state">
    /// - Unlocking
    /// - Unlocked
    /// - Locked
    /// </param>
    private void SetSessionState(AppSessionLockState state)
    {
        if (SessionState == state)
            return;

        try
        {
            SessionState = state;
            SessionStateChanged?.Invoke(this, state);
            RaiseCommandStates();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred in a subscriber of SessionStateChanged.");
        }
    }

    private void RaiseCommandStates()
    {
        if (WindowStateChangedCommand is RelayCommand<WindowState> windowStateChangedCommand)
        {
            windowStateChangedCommand.RaiseCanExecuteChanged();
        }

        if (DetachWindowCommand is RelayCommand detachWindowCommand)
        {
            detachWindowCommand.RaiseCanExecuteChanged();
        }
    }
}
