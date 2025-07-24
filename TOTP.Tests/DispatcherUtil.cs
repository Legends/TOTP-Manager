using System.Windows.Threading;

namespace TOTP.Tests;

public static class DispatcherUtil
{
    public static void DoEvents()
    {
        var frame = new DispatcherFrame();
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
            new Action(() => { frame.Continue = false; }));
        Dispatcher.PushFrame(frame);
    }
}