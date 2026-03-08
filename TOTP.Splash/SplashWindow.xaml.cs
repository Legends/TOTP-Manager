using System;
using System.Windows;

namespace TOTP.Splash;

public partial class SplashWindow : Window
{
    public SplashWindow()
    {
        InitializeComponent();
    }

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        DebugLog.Write("SplashWindow - ContentRendered");
    }
}
