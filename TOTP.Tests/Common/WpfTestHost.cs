using System.Windows;
using System.Windows.Threading;

namespace TOTP.Tests.Common;

internal static class WpfTestHost
{
    private static readonly TaskCompletionSource<Dispatcher> Ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private static readonly Thread HostThread;
    private static int _shutdownRequested;

    static WpfTestHost()
    {
        HostThread = new Thread(() =>
        {
            try
            {
                var application = Application.Current ?? new Application
                {
                    ShutdownMode = ShutdownMode.OnExplicitShutdown
                };

                AddResource(application, "/TOTP.UI.WPF;component/Styles/Theme.xaml");
                AddResource(application, "/TOTP.UI.WPF;component/Styles/Common.xaml");
                Ready.TrySetResult(Dispatcher.CurrentDispatcher);
                Dispatcher.Run();
            }
            catch (Exception ex)
            {
                Ready.TrySetException(ex);
            }
        })
        {
            IsBackground = true,
            Name = "TOTP Tests WPF Host"
        };
        HostThread.SetApartmentState(ApartmentState.STA);
        HostThread.Start();
    }

    public static void Run(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var dispatcher = Ready.Task.WaitAsync(TimeSpan.FromSeconds(15)).GetAwaiter().GetResult();
        dispatcher.Invoke(action);
    }

    public static void RunAsync(Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var dispatcher = Ready.Task.WaitAsync(TimeSpan.FromSeconds(15)).GetAwaiter().GetResult();
        dispatcher.InvokeAsync(action).Task.Unwrap().WaitAsync(TimeSpan.FromSeconds(30)).GetAwaiter().GetResult();
    }

    public static void Shutdown()
    {
        if (Interlocked.Exchange(ref _shutdownRequested, 1) != 0)
        {
            return;
        }

        var dispatcher = Ready.Task.WaitAsync(TimeSpan.FromSeconds(15)).GetAwaiter().GetResult();
        if (!dispatcher.HasShutdownStarted)
        {
            dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() =>
            {
                if (ReferenceEquals(Application.Current?.Dispatcher, dispatcher))
                {
                    Application.Current.Shutdown();
                }

                dispatcher.BeginInvokeShutdown(DispatcherPriority.Send);
            }));
        }

        if (!HostThread.Join(TimeSpan.FromSeconds(15)))
        {
            throw new TimeoutException("The WPF test dispatcher did not shut down within 15 seconds.");
        }
    }

    private static void AddResource(Application application, string uri)
    {
        if (application.Resources.MergedDictionaries.Any(dictionary =>
                string.Equals(dictionary.Source?.OriginalString, uri, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        application.Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri(uri, UriKind.Relative)
        });
    }
}
