using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TOTP.Commands;
using TOTP.Security.Interfaces;
using TOTP.Security.Models;
using TOTP.Services.Interfaces;

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

    public MainViewSessionController(
        IAuthorizationService authorization,
        IUserActivityService activityService,
        IInputActivityMonitor inputActivityMonitor)
    {
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
        SetSessionState(_authorization.State.IsUnlocked ? AppSessionState.Unlocked : AppSessionState.Locked);

        if (IsUnlocked)
        {
            if (_attachedWindow != null)
            {
                _inputActivityMonitor.Attach(_attachedWindow);
            }

            if (_onUnlockedAsync != null)
                await _onUnlockedAsync();

            UpdateActivityMonitorState();
        }
        else
        {
            _onLocked?.Invoke();
            UpdateActivityMonitorState();
            _inputActivityMonitor.Detach();
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
