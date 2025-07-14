using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Github2FA.Tests
{
    public static class DispatcherUtil
    {
        public static void DoEvents()
        {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                frame.Continue = false;
            }));
            Dispatcher.PushFrame(frame);
        }
    }

}
