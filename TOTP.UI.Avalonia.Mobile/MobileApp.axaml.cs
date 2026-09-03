using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using TOTP.Avalonia.Mobile.Views;

namespace TOTP.Avalonia.Mobile;

public partial class MobileApp : Application
{
    public Func<Control>? MainViewFactory { private get; set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IActivityApplicationLifetime activityLifetime)
        {
            activityLifetime.MainViewFactory = CreateMainView;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewLifetime)
        {
            singleViewLifetime.MainView = CreateMainView();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private Control CreateMainView() => MainViewFactory?.Invoke()
        ?? throw new InvalidOperationException("The mobile composition root was not configured.");
}
