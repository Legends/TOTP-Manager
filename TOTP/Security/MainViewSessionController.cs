using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TOTP.Commands;
using TOTP.Core.Enums;
using TOTP.Security.Interfaces;
using TOTP.Security.Models;
using TOTP.Services.Interfaces;

namespace TOTP.Security;

public sealed class MainViewSessionController : IMainViewSessionController
{
    private readonly IAuthorizationService _authorization;
    private readonly IInputActivityMonitor _inputActivityMonitor;
    private readonly ILogger<MainViewSessionController> _logger;

    private Func<Task>? _onUnlockedAsync;
    private Action? _onLocked;
    private IMainWindow? _attachedWindow;

    public AppSessionState SessionState { get; private set; } = AppSessionState.Locked;
    public bool IsUnlocked => SessionState == AppSessionState.Unlocked;

    public event EventHandler<AppSessionState>? SessionStateChanged;

    public ICommand WindowStateChangedCommand { get; }
    public ICommand DetachWindowCommand { get; }

    public MainViewSessionController(
        IAuthorizationService authorization,
        IInputActivityMonitor inputActivityMonitor,
        ILogger<MainViewSessionController> logger)
    {
        _logger = logger;
        _authorization = authorization;
        _inputActivityMonitor = inputActivityMonitor;

        WindowStateChangedCommand = new RelayCommand<WindowState>(OnWindowStateChanged);
        DetachWindowCommand = new RelayCommand(DetachWindow);

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
            SetSessionState(AppSessionState.Unlocking);

            await _authorization.InitializeAsync();

            var startupUnlockResult = await _authorization.TryUnlockOnStartupAsync();
            if (!IsUnlocked && startupUnlockResult != AuthorizationResult.Success)
            {
                SetSessionState(AppSessionState.Locked);
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Critical failure during session initialization.");
            SetSessionState(AppSessionState.Locked);
        }
    }

    public void AttachWindow(IMainWindow? window)
    {
        if (window == null) return;

        try
        {
            _attachedWindow = window;
            _inputActivityMonitor.Attach(window);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to attach window to activity monitor.");
        }
    }

    private void DetachWindow()
    {
        try
        {
            _inputActivityMonitor.Detach();
            _attachedWindow = null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error while detaching window.");
        }
    }

    public void Lock()
    {
        SetSessionState(AppSessionState.Locked);

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
        if (state == WindowState.Minimized)
            Lock();
    }

    private async void AuthorizationState_Changed(object? sender, EventArgs e)
    {
        try
        {
            SetSessionState(_authorization.State.IsUnlocked ? AppSessionState.Unlocked : AppSessionState.Locked);

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
                SetSessionState(AppSessionState.Locked);
                _onLocked?.Invoke();
            }
        }
    }

    private void SetSessionState(AppSessionState state)
    {
        if (SessionState == state)
            return;

        try
        {
            SessionState = state;
            SessionStateChanged?.Invoke(this, state);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred in a subscriber of SessionStateChanged.");
        }
    }
}