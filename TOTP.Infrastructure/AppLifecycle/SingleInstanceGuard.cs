using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TOTP.Infrastructure.AppLifecycle;

public class SingleInstanceGuard : IDisposable
{
    private readonly Mutex _mutex;
    public bool IsFirstInstance { get; }

    public SingleInstanceGuard(string mutexName)
    {
        _mutex = new Mutex(true, mutexName, out bool isNew);
        IsFirstInstance = isNew;
    }

    public void Dispose()
    {
        if (IsFirstInstance)
        {
            _mutex.ReleaseMutex();
        }
        _mutex.Dispose();
    }

    public static void ActivateExistingWindow(string processName)
    {
        var current = Process.GetCurrentProcess();
        foreach (var process in Process.GetProcessesByName(processName))
        {
            if (process.Id != current.Id)
            {
                IntPtr hWnd = process.MainWindowHandle;
                if (hWnd != IntPtr.Zero)
                {
                    ShowWindow(hWnd, SW_RESTORE);
                    SetForegroundWindow(hWnd);
                }
            }
        }
    }

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int SW_RESTORE = 9;
}