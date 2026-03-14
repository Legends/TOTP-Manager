using TOTP.Infrastructure;

namespace TOTP.Tests.Infrastructure;

public sealed class SingleInstanceGuardTests
{
    [Fact]
    public void SelectBestWindowHandle_PrefersFirstUnownedWindow()
    {
        var handles = new[]
        {
            new SingleInstanceGuard.WindowHandleCandidate(new IntPtr(11), new IntPtr(99)),
            new SingleInstanceGuard.WindowHandleCandidate(new IntPtr(22), IntPtr.Zero),
            new SingleInstanceGuard.WindowHandleCandidate(new IntPtr(33), IntPtr.Zero)
        };

        var selected = SingleInstanceGuard.SelectBestWindowHandle(handles, new IntPtr(44));

        Assert.Equal(new IntPtr(22), selected);
    }

    [Fact]
    public void SelectBestWindowHandle_UsesMainWindowHandleWhenNoVisibleCandidatesExist()
    {
        var selected = SingleInstanceGuard.SelectBestWindowHandle([], new IntPtr(44));

        Assert.Equal(new IntPtr(44), selected);
    }

    [Fact]
    public void ActivateWindow_InvokesForegroundSequence_AndDetachesThreadsAfterward()
    {
        var api = new FakeSingleInstanceWindowApi
        {
            IsIconicResult = true,
            ForegroundWindow = new IntPtr(77),
            CurrentThreadId = 10,
            TargetThreadId = 20,
            ForegroundThreadId = 30
        };

        SingleInstanceGuard.ActivateWindow(new IntPtr(55), api);

        Assert.Equal(
            [
                "ShowWindowAsync:55:9",
                "AttachThreadInput:10:30:True",
                "AttachThreadInput:10:20:True",
                "BringWindowToTop:55",
                "SetForegroundWindow:55",
                "SetActiveWindow:55",
                "SetFocus:55",
                "AttachThreadInput:10:30:False",
                "AttachThreadInput:10:20:False"
            ],
            api.Calls);
    }

    private sealed class FakeSingleInstanceWindowApi : ISingleInstanceWindowApi
    {
        public List<string> Calls { get; } = [];
        public bool IsIconicResult { get; set; }
        public IntPtr ForegroundWindow { get; set; }
        public uint CurrentThreadId { get; set; }
        public uint TargetThreadId { get; set; }
        public uint ForegroundThreadId { get; set; }

        public bool EnumWindows(Func<IntPtr, IntPtr, bool> callback, IntPtr lParam) => true;

        public bool SetForegroundWindow(IntPtr hWnd)
        {
            Calls.Add($"SetForegroundWindow:{hWnd}");
            return true;
        }

        public bool ShowWindowAsync(IntPtr hWnd, int nCmdShow)
        {
            Calls.Add($"ShowWindowAsync:{hWnd}:{nCmdShow}");
            return true;
        }

        public bool BringWindowToTop(IntPtr hWnd)
        {
            Calls.Add($"BringWindowToTop:{hWnd}");
            return true;
        }

        public IntPtr SetActiveWindow(IntPtr hWnd)
        {
            Calls.Add($"SetActiveWindow:{hWnd}");
            return hWnd;
        }

        public IntPtr SetFocus(IntPtr hWnd)
        {
            Calls.Add($"SetFocus:{hWnd}");
            return hWnd;
        }

        public IntPtr GetForegroundWindow() => ForegroundWindow;
        public bool IsWindowVisible(IntPtr hWnd) => true;
        public bool IsIconic(IntPtr hWnd) => IsIconicResult;
        public IntPtr GetWindow(IntPtr hWnd, uint uCmd) => IntPtr.Zero;

        public uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId)
        {
            if (hWnd == ForegroundWindow)
            {
                lpdwProcessId = 2;
                return ForegroundThreadId;
            }

            lpdwProcessId = 1;
            return TargetThreadId;
        }

        public uint GetCurrentThreadId() => CurrentThreadId;

        public bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach)
        {
            Calls.Add($"AttachThreadInput:{idAttach}:{idAttachTo}:{fAttach}");
            return true;
        }
    }
}
