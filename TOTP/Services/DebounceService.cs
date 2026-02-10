using System;
using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Threading;
using TOTP.Services.Interfaces;

namespace TOTP.Services;

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
            // make sure we are on the UI Dispatcher
            timer = new DispatcherTimer(DispatcherPriority.Normal, Application.Current.Dispatcher);
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
        if (_timers.TryRemove(key, out var timer)) timer.Stop();
    }
}