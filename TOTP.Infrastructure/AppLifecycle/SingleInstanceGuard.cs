using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TOTP.Infrastructure.AppLifecycle;

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
        if (_disposed) return;
        _disposed = true;

        var m = Interlocked.Exchange(ref _mutex, null);
        if (m is null) return;

        try
        {
            if (_owns)
                m.ReleaseMutex();
        }
        catch (ApplicationException) { /* not owner */ }

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
