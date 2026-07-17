using TOTP.Core.Platform;
using TOTP.Presentation.Platform;

namespace TOTP.Tests.Infrastructure;

public sealed class WindowsExistingInstanceActivatorTests
{
    [Fact]
    public void TryDispatch_WhenPayloadVersionIsUnsupported_DoesNotUseWindowApi()
    {
        var api = new FakeSingleInstanceWindowApi();
        var sut = new WindowsExistingInstanceActivator("TOTP.UI.WPF", api);

        var result = sut.TryDispatch(new ApplicationActivationRequest(
            99,
            ApplicationActivationKind.ActivateMainWindow));

        Assert.False(result);
        Assert.Empty(api.Calls);
    }

    [Fact]
    public void NativeApi_RestoreEntryPointsResolve()
    {
        var api = new WindowsInstanceWindowApi();

        Assert.False(api.IsIconic(IntPtr.Zero));
        Assert.False(api.ShowWindow(IntPtr.Zero, 9));
    }

    [Fact]
    public void SelectBestWindowHandle_PrefersFirstUnownedWindow()
    {
        var handles = new[]
        {
            new WindowsExistingInstanceActivator.WindowHandleCandidate(new IntPtr(11), new IntPtr(99)),
            new WindowsExistingInstanceActivator.WindowHandleCandidate(new IntPtr(22), IntPtr.Zero),
            new WindowsExistingInstanceActivator.WindowHandleCandidate(new IntPtr(33), IntPtr.Zero)
        };

        var selected = WindowsExistingInstanceActivator.SelectBestWindowHandle(handles, new IntPtr(44));

        Assert.Equal(new IntPtr(22), selected);
    }

    [Fact]
    public void SelectBestWindowHandle_UsesMainWindowHandleWhenNoVisibleCandidatesExist()
    {
        var selected = WindowsExistingInstanceActivator.SelectBestWindowHandle([], new IntPtr(44));

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

        WindowsExistingInstanceActivator.ActivateWindow(new IntPtr(55), api);

        Assert.Equal(
            [
                "ShowWindow:55:9",
                "AttachThreadInput:10:30:True",
                "AttachThreadInput:10:20:True",
                "SetWindowPos:55:-1:67",
                "SetWindowPos:55:-2:67",
                "BringWindowToTop:55",
                "SetForegroundWindow:55",
                "SetActiveWindow:55",
                "SetFocus:55",
                "AttachThreadInput:10:30:False",
                "AttachThreadInput:10:20:False"
            ],
            api.Calls);
    }

    private sealed class FakeSingleInstanceWindowApi : IWindowsInstanceWindowApi
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

        public bool ShowWindow(IntPtr hWnd, int nCmdShow)
        {
            Calls.Add($"ShowWindow:{hWnd}:{nCmdShow}");
            return true;
        }

        public bool BringWindowToTop(IntPtr hWnd)
        {
            Calls.Add($"BringWindowToTop:{hWnd}");
            return true;
        }

        public bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int width, int height, uint flags)
        {
            Calls.Add($"SetWindowPos:{hWnd}:{hWndInsertAfter}:{flags}");
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
