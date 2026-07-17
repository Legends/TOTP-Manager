using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using TOTP.Core.Platform;

namespace TOTP.Presentation.Platform;

public sealed class WindowsExistingInstanceActivator : IActivationDispatcher
{
    private const int SW_SHOW = 5;
    private const int SW_RESTORE = 9;
    private const uint GW_OWNER = 4;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_SHOWWINDOW = 0x0040;
    private static readonly IntPtr HwndTopmost = new(-1);
    private static readonly IntPtr HwndNotTopmost = new(-2);
    private static readonly IWindowsInstanceWindowApi WindowApi = new WindowsInstanceWindowApi();
    private readonly string _processName;
    private readonly IWindowsInstanceWindowApi _windowApi;

    public WindowsExistingInstanceActivator(string processName)
        : this(processName, WindowApi)
    {
    }

    internal WindowsExistingInstanceActivator(string processName, IWindowsInstanceWindowApi windowApi)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(processName);
        _processName = processName;
        _windowApi = windowApi;
    }

    public bool TryDispatch(ApplicationActivationRequest request)
    {
        if (!request.IsSupported || request.Kind != ApplicationActivationKind.ActivateMainWindow)
        {
            return false;
        }

        var activated = false;
        using var current = Process.GetCurrentProcess();
        foreach (var process in Process.GetProcessesByName(_processName))
        {
            using (process)
            {
                if (process.Id == current.Id)
                {
                    continue;
                }

                var windowHandle = FindBestWindowHandle(process, _windowApi);
                if (windowHandle != IntPtr.Zero)
                {
                    ActivateWindow(windowHandle, _windowApi);
                    activated = true;
                }
            }
        }

        return activated;
    }

    internal static IntPtr FindBestWindowHandle(Process process, IWindowsInstanceWindowApi windowApi)
    {
        var handles = new List<WindowHandleCandidate>();
        windowApi.EnumWindows((hWnd, _) =>
        {
            windowApi.GetWindowThreadProcessId(hWnd, out var processId);
            if (processId == process.Id && windowApi.IsWindowVisible(hWnd))
            {
                handles.Add(new WindowHandleCandidate(hWnd, windowApi.GetWindow(hWnd, GW_OWNER)));
            }

            return true;
        }, IntPtr.Zero);

        return SelectBestWindowHandle(handles, process.MainWindowHandle);
    }

    internal static IntPtr SelectBestWindowHandle(IReadOnlyList<WindowHandleCandidate> handles, IntPtr mainWindowHandle)
    {
        if (handles.Count == 0)
        {
            return mainWindowHandle;
        }

        foreach (var handle in handles)
        {
            if (handle.Owner == IntPtr.Zero)
            {
                return handle.Handle;
            }
        }

        return handles[0].Handle;
    }

    internal static void ActivateWindow(IntPtr hWnd, IWindowsInstanceWindowApi windowApi)
    {
        if (hWnd == IntPtr.Zero)
        {
            return;
        }

        // Restore synchronously before changing Z-order. With ShowWindowAsync the
        // foreground promotion can run while the target window is still minimized.
        windowApi.ShowWindow(hWnd, windowApi.IsIconic(hWnd) ? SW_RESTORE : SW_SHOW);

        var currentThreadId = windowApi.GetCurrentThreadId();
        var targetThreadId = windowApi.GetWindowThreadProcessId(hWnd, out _);
        var foregroundWindow = windowApi.GetForegroundWindow();
        var foregroundThreadId = foregroundWindow == IntPtr.Zero
            ? 0
            : windowApi.GetWindowThreadProcessId(foregroundWindow, out _);

        try
        {
            if (foregroundThreadId != 0)
            {
                windowApi.AttachThreadInput(currentThreadId, foregroundThreadId, true);
            }

            if (targetThreadId != 0)
            {
                windowApi.AttachThreadInput(currentThreadId, targetThreadId, true);
            }

            const uint activationFlags = SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW;
            windowApi.SetWindowPos(hWnd, HwndTopmost, 0, 0, 0, 0, activationFlags);
            windowApi.SetWindowPos(hWnd, HwndNotTopmost, 0, 0, 0, 0, activationFlags);
            windowApi.BringWindowToTop(hWnd);
            windowApi.SetForegroundWindow(hWnd);
            windowApi.SetActiveWindow(hWnd);
            windowApi.SetFocus(hWnd);
        }
        finally
        {
            if (foregroundThreadId != 0)
            {
                windowApi.AttachThreadInput(currentThreadId, foregroundThreadId, false);
            }

            if (targetThreadId != 0)
            {
                windowApi.AttachThreadInput(currentThreadId, targetThreadId, false);
            }
        }
    }

    internal readonly record struct WindowHandleCandidate(IntPtr Handle, IntPtr Owner);
}

internal interface IWindowsInstanceWindowApi
{
    bool EnumWindows(Func<IntPtr, IntPtr, bool> callback, IntPtr lParam);
    bool SetForegroundWindow(IntPtr hWnd);
    bool ShowWindow(IntPtr hWnd, int nCmdShow);
    bool BringWindowToTop(IntPtr hWnd);
    bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int width, int height, uint flags);
    IntPtr SetActiveWindow(IntPtr hWnd);
    IntPtr SetFocus(IntPtr hWnd);
    IntPtr GetForegroundWindow();
    bool IsWindowVisible(IntPtr hWnd);
    bool IsIconic(IntPtr hWnd);
    IntPtr GetWindow(IntPtr hWnd, uint uCmd);
    uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
    uint GetCurrentThreadId();
    bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);
}

internal sealed class WindowsInstanceWindowApi : IWindowsInstanceWindowApi
{
    public bool EnumWindows(Func<IntPtr, IntPtr, bool> callback, IntPtr lParam)
    {
        return EnumWindowsNative((hWnd, param) => callback(hWnd, param), lParam);
    }

    public bool SetForegroundWindow(IntPtr hWnd) => SetForegroundWindowNative(hWnd);
    public bool ShowWindow(IntPtr hWnd, int nCmdShow) => ShowWindowNative(hWnd, nCmdShow);
    public bool BringWindowToTop(IntPtr hWnd) => BringWindowToTopNative(hWnd);
    public bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int width, int height, uint flags)
        => SetWindowPosNative(hWnd, hWndInsertAfter, x, y, width, height, flags);
    public IntPtr SetActiveWindow(IntPtr hWnd) => SetActiveWindowNative(hWnd);
    public IntPtr SetFocus(IntPtr hWnd) => SetFocusNative(hWnd);
    public IntPtr GetForegroundWindow() => GetForegroundWindowNative();
    public bool IsWindowVisible(IntPtr hWnd) => IsWindowVisibleNative(hWnd);
    public bool IsIconic(IntPtr hWnd) => IsIconicNative(hWnd);
    public IntPtr GetWindow(IntPtr hWnd, uint uCmd) => GetWindowNative(hWnd, uCmd);
    public uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId) => GetWindowThreadProcessIdNative(hWnd, out lpdwProcessId);
    public uint GetCurrentThreadId() => GetCurrentThreadIdNative();
    public bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach) => AttachThreadInputNative(idAttach, idAttachTo, fAttach);

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll", EntryPoint = "EnumWindows")]
    private static extern bool EnumWindowsNative(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll", EntryPoint = "SetForegroundWindow")]
    private static extern bool SetForegroundWindowNative(IntPtr hWnd);

    [DllImport("user32.dll", EntryPoint = "ShowWindow")]
    private static extern bool ShowWindowNative(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll", EntryPoint = "BringWindowToTop")]
    private static extern bool BringWindowToTopNative(IntPtr hWnd);

    [DllImport("user32.dll", EntryPoint = "SetWindowPos", SetLastError = true)]
    private static extern bool SetWindowPosNative(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int x,
        int y,
        int width,
        int height,
        uint flags);

    [DllImport("user32.dll", EntryPoint = "SetActiveWindow")]
    private static extern IntPtr SetActiveWindowNative(IntPtr hWnd);

    [DllImport("user32.dll", EntryPoint = "SetFocus")]
    private static extern IntPtr SetFocusNative(IntPtr hWnd);

    [DllImport("user32.dll", EntryPoint = "GetForegroundWindow")]
    private static extern IntPtr GetForegroundWindowNative();

    [DllImport("user32.dll", EntryPoint = "IsWindowVisible")]
    private static extern bool IsWindowVisibleNative(IntPtr hWnd);

    [DllImport("user32.dll", EntryPoint = "IsIconic")]
    private static extern bool IsIconicNative(IntPtr hWnd);

    [DllImport("user32.dll", EntryPoint = "GetWindow")]
    private static extern IntPtr GetWindowNative(IntPtr hWnd, uint uCmd);

    [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId")]
    private static extern uint GetWindowThreadProcessIdNative(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("kernel32.dll", EntryPoint = "GetCurrentThreadId")]
    private static extern uint GetCurrentThreadIdNative();

    [DllImport("user32.dll", EntryPoint = "AttachThreadInput")]
    private static extern bool AttachThreadInputNative(uint idAttach, uint idAttachTo, bool fAttach);
}
