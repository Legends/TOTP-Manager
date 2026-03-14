using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace TOTP.Infrastructure;

public sealed class SingleInstanceGuard : IDisposable
{
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

            var windowHandle = FindBestWindowHandle(process);
            if (windowHandle != IntPtr.Zero)
            {
                ActivateWindow(windowHandle);
            }
        }
    }

    private static IntPtr FindBestWindowHandle(Process process)
    {
        var handles = new List<IntPtr>();
        EnumWindows((hWnd, _) =>
        {
            GetWindowThreadProcessId(hWnd, out var processId);
            if (processId == process.Id && IsWindowVisible(hWnd))
            {
                handles.Add(hWnd);
            }

            return true;
        }, IntPtr.Zero);

        if (handles.Count == 0)
        {
            return process.MainWindowHandle;
        }

        foreach (var handle in handles)
        {
            if (GetWindow(handle, GW_OWNER) == IntPtr.Zero)
            {
                return handle;
            }
        }

        return handles[0];
    }

    private static void ActivateWindow(IntPtr hWnd)
    {
        if (hWnd == IntPtr.Zero)
        {
            return;
        }

        ShowWindowAsync(hWnd, IsIconic(hWnd) ? SW_RESTORE : SW_SHOW);

        var currentThreadId = GetCurrentThreadId();
        var targetThreadId = GetWindowThreadProcessId(hWnd, out _);
        var foregroundWindow = GetForegroundWindow();
        var foregroundThreadId = foregroundWindow == IntPtr.Zero
            ? 0
            : GetWindowThreadProcessId(foregroundWindow, out _);

        try
        {
            if (foregroundThreadId != 0)
            {
                AttachThreadInput(currentThreadId, foregroundThreadId, true);
            }

            if (targetThreadId != 0)
            {
                AttachThreadInput(currentThreadId, targetThreadId, true);
            }

            BringWindowToTop(hWnd);
            SetForegroundWindow(hWnd);
            SetActiveWindow(hWnd);
            SetFocus(hWnd);
        }
        finally
        {
            if (foregroundThreadId != 0)
            {
                AttachThreadInput(currentThreadId, foregroundThreadId, false);
            }

            if (targetThreadId != 0)
            {
                AttachThreadInput(currentThreadId, targetThreadId, false);
            }
        }
    }

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool BringWindowToTop(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr SetActiveWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr SetFocus(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    [DllImport("user32.dll")]
    private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

    private const int SW_SHOW = 5;
    private const int SW_RESTORE = 9;
    private const uint GW_OWNER = 4;
}
