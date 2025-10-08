using Syncfusion.Windows.Shared;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using TOTP.Events;
using TOTP.Helper;
using TOTP.Interfaces;

namespace TOTP.UserControls;

public partial class UserMessageDialog : ChromelessWindow
{
    public bool Result { get; private set; }

    public UserMessageDialog(IUserMessageDialogViewModel vm)
    {
        InitializeComponent();
        Opacity = 0; // start transparent

        DataContext = vm;

        TitleBarBackground = vm.TitleBarBackground;
        if (!string.IsNullOrWhiteSpace(vm.IconPath) && Common.PackUriExists(vm.IconPath))
            Icon = new BitmapImage(new Uri(vm.IconPath));

        vm.RequestClose += OnRequestClose;
        Closed += (_, _) => vm.RequestClose -= OnRequestClose;

        ContentRendered += (_, _) => BeginFadeIn();
    }

    private void BeginFadeIn()
    {
        var fadeIn = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromSeconds(0.35),
            EasingFunction = new QuadraticEase()
        };
        BeginAnimation(Window.OpacityProperty, fadeIn);
    }

    private async Task BeginFadeOutAsync()
    {
        var fadeOut = new DoubleAnimation
        {
            From = Opacity,
            To = 0,
            Duration = TimeSpan.FromSeconds(0.25),
            EasingFunction = new QuadraticEase()
        };

        var tcs = new TaskCompletionSource();
        fadeOut.Completed += (_, _) => tcs.SetResult();

        BeginAnimation(Window.OpacityProperty, fadeOut);
        await tcs.Task; // wait for fade to complete
    }

    private async void OnRequestClose(object? sender, DialogCloseRequestedEventArgs e)
    {
        Result = e.DialogResult;
        DialogResult = e.DialogResult;

        await BeginFadeOutAsync();
        Close();
    }

    protected override async void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        // only fade out if still visible
        if (Opacity > 0.1 && !_isClosingFromAnimation)
        {
            e.Cancel = true;
            await BeginFadeOutAsync();
            _isClosingFromAnimation = true;
            Close();
        }
        base.OnClosing(e);
    }

    private bool _isClosingFromAnimation;
}
