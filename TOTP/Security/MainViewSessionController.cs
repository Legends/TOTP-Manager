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
using TOTP.ViewModels;

namespace TOTP.Security;

public sealed class MainViewSessionController : IMainViewSessionController
{
    private readonly IAuthorizationService _authorization;
    private readonly IUserActivityService _activityService;
    private readonly IInputActivityMonitor _inputActivityMonitor;

    private Func<Task>? _onUnlockedAsync;
    private Action? _onLocked;
    private IMainWindow? _attachedWindow;

    public AppSessionState SessionState { get; private set; } = AppSessionState.Locked;
    public bool IsUnlocked => SessionState == AppSessionState.Unlocked;

    public event EventHandler<AppSessionState>? SessionStateChanged;

    public ICommand WindowStateChangedCommand { get; }
    public ICommand DetachWindowCommand { get; }

    private ILogger<MainViewSessionController> _logger;

    public MainViewSessionController(
        IAuthorizationService authorization,
        IUserActivityService activityService,
        IInputActivityMonitor inputActivityMonitor, ILogger<MainViewSessionController> logger)
    {
        _logger = logger;
        _authorization = authorization;
        WindowStateChangedCommand = new RelayCommand<WindowState>(OnWindowStateChanged);
        DetachWindowCommand = new RelayCommand(DetachWindow);
        _activityService = activityService;
        _inputActivityMonitor = inputActivityMonitor;

        _authorization.State.Changed += AuthorizationState_Changed;
        _activityService.LockRequested += ActivityService_LockRequested;
    }

    public void ConfigureCallbacks(Func<Task> onUnlockedAsync, Action onLocked)
    {
        _onUnlockedAsync = onUnlockedAsync;
        _onLocked = onLocked;
    }

    public async Task InitializeAsync(IMainWindow? mainWindow)
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

    public void AttachWindow(IMainWindow? window)
    {
        if (window == null)
            return;

        _attachedWindow = window;
        _inputActivityMonitor.Attach(window);
        UpdateActivityMonitorState();
    }

    private void DetachWindow()
    {
        _inputActivityMonitor.Detach();
        _attachedWindow = null;
    }

    public void Lock()
    {
        SetSessionState(AppSessionState.Locked);
        _authorization.Lock();
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
            // 1. Update the session state immediately
            SetSessionState(_authorization.State.IsUnlocked ? AppSessionState.Unlocked : AppSessionState.Locked);

            if (IsUnlocked)
            {
                if (_attachedWindow != null)
                {
                    _inputActivityMonitor.Attach(_attachedWindow);
                }

                // 2. Await the async callback safely
                if (_onUnlockedAsync != null)
                {
                    await _onUnlockedAsync();
                }

                UpdateActivityMonitorState();
            }
            else
            {
                _onLocked?.Invoke();
                UpdateActivityMonitorState();
                _inputActivityMonitor.Detach();
            }
        }
        catch (Exception ex)
        {
            // 3. Log the error - since this is a session state change, 
            // a failure here is high priority.
            _logger.LogError(ex, "Error occurred during AuthorizationState change.");

            // 4. Optional: Force a Lock state if an error occurs during unlock 
            // to prevent the app from getting stuck in an inconsistent state.
            if (IsUnlocked)
            {
                SetSessionState(AppSessionState.Locked);
                _onLocked?.Invoke();
            }

            // 5. Notify the user or show a global error dialog if necessary
            //OnMessageSend?.Invoke(this, OperationStatus.Unknown, "An error occurred while updating the session.");
        }
    }

    private void ActivityService_LockRequested(object? sender, EventArgs e)
    {
        Lock();
    }

    private void UpdateActivityMonitorState()
    {
        if (IsUnlocked)
            _activityService.StartMonitoring();
        else
            _activityService.StopMonitoring();
    }

    private void SetSessionState(AppSessionState state)
    {
        if (SessionState == state)
            return;

        SessionState = state;
        SessionStateChanged?.Invoke(this, state);
    }
}
