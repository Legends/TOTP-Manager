using NetSparkleUpdater;
using NetSparkleUpdater.Events;
using System.Threading;
using Xunit;
using Xunit.StaFact;

namespace TOTP.Tests.AutoUpdate;

public sealed class TOTPDownloadProgressWindowTests
{
    [StaFact]
    public void FinishedDownloadingFile_ValidDownload_BecomesReadyEvenWithoutFinal100PercentProgressEvent()
    {
        var sut = new TOTPDownloadProgressWindow(new AppCastItem
        {
            ShortVersion = "1.0.0.51",
            DownloadLink = "https://example.invalid/TOTP-Manager-fast.zip",
            UpdateSize = 54239853
        });

        sut.OnDownloadProgressChanged(this, new ItemDownloadProgressEventArgs(54, this, 29816654, 54239853));
        sut.FinishedDownloadingFile(true);

        Thread.Sleep(1000);
        DispatcherUtil.DoEvents();

        var title = (System.Windows.Controls.TextBlock)sut.FindName("TitleText");
        var state = (System.Windows.Controls.TextBlock)sut.FindName("ProgressStateText");
        var progress = (System.Windows.Controls.TextBlock)sut.FindName("ProgressText");
        var button = (System.Windows.Controls.Button)sut.FindName("ActionButton");
        var bar = (System.Windows.Controls.ProgressBar)sut.FindName("DownloadProgressBar");

        Assert.Equal("Update ready to install", title.Text);
        Assert.Equal("Ready", state.Text);
        Assert.Equal("The download completed and passed signature verification.", progress.Text);
        Assert.Equal("Install update", button.Content);
        Assert.True(button.IsEnabled);
        Assert.Equal(100, bar.Value);
        Assert.False(bar.IsIndeterminate);
    }

    [StaFact]
    public void DisplayErrorMessage_MakesErrorStateTerminal()
    {
        var sut = new TOTPDownloadProgressWindow(new AppCastItem
        {
            ShortVersion = "1.0.0.51",
            DownloadLink = "https://example.invalid/TOTP-Manager-fast.zip",
            UpdateSize = 54239853
        });

        sut.OnDownloadProgressChanged(this, new ItemDownloadProgressEventArgs(52, this, 28637006, 54239853));
        sut.DisplayErrorMessage("The response ended prematurely.");
        sut.OnDownloadProgressChanged(this, new ItemDownloadProgressEventArgs(99, this, 50000000, 54239853));
        sut.FinishedDownloadingFile(true);

        var title = (System.Windows.Controls.TextBlock)sut.FindName("TitleText");
        var state = (System.Windows.Controls.TextBlock)sut.FindName("ProgressStateText");
        var progress = (System.Windows.Controls.TextBlock)sut.FindName("ProgressText");
        var error = (System.Windows.Controls.TextBlock)sut.FindName("ErrorText");
        var button = (System.Windows.Controls.Button)sut.FindName("ActionButton");
        var bar = (System.Windows.Controls.ProgressBar)sut.FindName("DownloadProgressBar");

        Assert.Equal("Download interrupted", title.Text);
        Assert.Equal("Error", state.Text);
        Assert.Equal("The update package could not be downloaded completely.", progress.Text);
        Assert.Equal("The response ended prematurely.", error.Text);
        Assert.Equal(System.Windows.Visibility.Visible, error.Visibility);
        Assert.Equal("Close", button.Content);
        Assert.True(button.IsEnabled);
        Assert.Equal(52, bar.Value);
        Assert.False(bar.IsIndeterminate);
    }

    [StaFact]
    public void SetDownloadAndInstallButtonEnabled_DoesNotDisableButtonAfterReadyState()
    {
        var sut = new TOTPDownloadProgressWindow(new AppCastItem
        {
            ShortVersion = "1.0.0.51",
            DownloadLink = "https://example.invalid/TOTP-Manager-fast.zip",
            UpdateSize = 54239853
        });

        sut.FinishedDownloadingFile(true);

        Thread.Sleep(1000);
        DispatcherUtil.DoEvents();

        sut.SetDownloadAndInstallButtonEnabled(false);

        var button = (System.Windows.Controls.Button)sut.FindName("ActionButton");
        Assert.True(button.IsEnabled);
        Assert.Equal("Install update", button.Content);
    }
}
