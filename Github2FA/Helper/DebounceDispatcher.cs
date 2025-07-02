using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Github2FA.Helper
{
    public class DebounceDispatcher
    {
        private DispatcherTimer? _timer;
        private Action _action;

        public void Debounce(int milliseconds, Action action)
        {
            _timer?.Stop();
            _action = action;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(milliseconds) };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            _timer?.Stop();
            _timer.Tick -= Timer_Tick;
            _action?.Invoke();
        }
    }

}
