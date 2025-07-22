namespace Github2FA.Services;

using Github2FA.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Windows.Threading;

public class DebounceService : IDebounceService
{
    private readonly ConcurrentDictionary<string, DispatcherTimer> _timers = new();

    public void Debounce(string key, int milliseconds, Action action)
    {
        if (_timers.TryGetValue(key, out var timer))
        {
            timer.Stop();
        }
        else
        {
            timer = new DispatcherTimer();
            _timers[key] = timer;
        }

        timer.Interval = TimeSpan.FromMilliseconds(milliseconds);
        timer.Tick -= OnTimerTick; // Prevent duplicate
        timer.Tick += OnTimerTick;

        void OnTimerTick(object? sender, EventArgs e)
        {
            timer.Stop();
            timer.Tick -= OnTimerTick;
            _timers.TryRemove(key, out _);
            action();
        }

        timer.Start();
    }

    public void Cancel(string key)
    {
        if (_timers.TryRemove(key, out var timer))
        {
            timer.Stop();
        }
    }
}
