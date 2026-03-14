using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace TOTP.Infrastructure;

public sealed class SingleInstanceGuard : IDisposable
{
    private const int SW_SHOW = 5;
    private const int SW_RESTORE = 9;
    private const uint GW_OWNER = 4;
    private static readonly ISingleInstanceWindowApi WindowApi = new SingleInstanceWindowApi();
    private Mutex? _mutex;
    private readonly bool _owns;
    private bool _disposed;

    public SingleInstanceGuard(string name)
    {
        _mutex = new Mutex(initiallyOwned: true, name: $@"Global\{name}", out bool createdNew);
        _owns = createdNew;
    }

    public bool IsFirstInstance => _owns;

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        var mutex = Interlocked.Exchange(ref _mutex, null);
        if (mutex is null)
        {
            return;
        }

        try
        {
            if (_owns)
            {
                mutex.ReleaseMutex();
            }
        }
        catch (ApplicationException ex)
        {
            Trace.TraceWarning($"SingleInstanceGuard.ReleaseMutex skipped because current thread does not own mutex: {ex.Message}");
        }
    }

    public static void ActivateExistingWindow(string processName)
    {
        var current = Process.GetCurrentProcess();
        foreach (var process in Process.GetProcessesByName(processName))
        {
            if (process.Id == current.Id)
            {
                continue;
            }

            var windowHandle = FindBestWindowHandle(process, WindowApi);
            if (windowHandle != IntPtr.Zero)
            {
                ActivateWindow(windowHandle, WindowApi);
            }
        }
    }

    internal static IntPtr FindBestWindowHandle(Process process, ISingleInstanceWindowApi windowApi)
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

    internal static void ActivateWindow(IntPtr hWnd, ISingleInstanceWindowApi windowApi)
    {
        if (hWnd == IntPtr.Zero)
        {
            return;
        }

        windowApi.ShowWindowAsync(hWnd, windowApi.IsIconic(hWnd) ? SW_RESTORE : SW_SHOW);

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

internal interface ISingleInstanceWindowApi
{
    bool EnumWindows(Func<IntPtr, IntPtr, bool> callback, IntPtr lParam);
    bool SetForegroundWindow(IntPtr hWnd);
    bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
    bool BringWindowToTop(IntPtr hWnd);
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

internal sealed class SingleInstanceWindowApi : ISingleInstanceWindowApi
{
    public bool EnumWindows(Func<IntPtr, IntPtr, bool> callback, IntPtr lParam)
    {
        return EnumWindowsNative((hWnd, param) => callback(hWnd, param), lParam);
    }

    public bool SetForegroundWindow(IntPtr hWnd) => SetForegroundWindowNative(hWnd);
    public bool ShowWindowAsync(IntPtr hWnd, int nCmdShow) => ShowWindowAsyncNative(hWnd, nCmdShow);
    public bool BringWindowToTop(IntPtr hWnd) => BringWindowToTopNative(hWnd);
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

    [DllImport("user32.dll")]
    private static extern bool EnumWindowsNative(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindowNative(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindowAsyncNative(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool BringWindowToTopNative(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr SetActiveWindowNative(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr SetFocusNative(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindowNative();

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisibleNative(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsIconicNative(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindowNative(IntPtr hWnd, uint uCmd);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessIdNative(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadIdNative();

    [DllImport("user32.dll")]
    private static extern bool AttachThreadInputNative(uint idAttach, uint idAttachTo, bool fAttach);
}
