using System;
using System.Windows.Threading;

namespace TOTP.Helper;

public class DebounceDispatcher
{
    private DispatcherTimer? _timer;
    private Action? _action;

    /// <summary>
    /// Debounce is called repeatedly when typing into the textbox, once for each character.
    /// so if u type multiple times, within the soecifed threshold specified in ms, 
    /// the timer will resest and start again and execute the action 
    /// after the last typed character when the specified timeinterval has elapsed 
    /// thats how debounce works.
    /// </summary>
    /// <param name="milliseconds"></param>
    /// <param name="action"></param>
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
