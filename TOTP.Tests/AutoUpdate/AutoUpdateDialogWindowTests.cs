using NetSparkleUpdater;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.Events;
using System.Threading;
using System.Windows;
using TOTP.AutoUpdate;
using Xunit;
using Xunit.Sdk;

namespace TOTP.Tests.AutoUpdate;

public sealed class AutoUpdateDialogWindowTests
{
    [Fact]
    public void FinishedDownloadingFile_ValidDownload_BecomesReadyEvenWithoutFinal100PercentProgressEvent()
    {
        RunInSta(() =>
        {
            EnsureAppResources();
            var sut = new AutoUpdateDialogWindow
            {
                SuppressPresentation = true
            };

            sut.ShowDownloadProgress(new AppCastItem
            {
                ShortVersion = "1.0.0.51",
                Version = "1.0.0.51",
                DownloadLink = "https://example.invalid/TOTP-Manager-fast.zip",
                UpdateSize = 54239853
            });

            sut.OnDownloadProgressChanged(new ItemDownloadProgressEventArgs(54, this, 29816654, 54239853));
            sut.FinishedDownloadingFile(true);

            Thread.Sleep(1000);
            DispatcherUtil.DoEvents();

            Assert.Equal("Update ready to install", sut.State.ProgressTitleText);
            Assert.Equal("Ready", sut.State.ProgressStateText);
            Assert.Equal("The download completed and passed signature verification.", sut.State.ProgressDescriptionText);
            Assert.Equal("Install update", sut.State.ProgressActionText);
            Assert.True(sut.State.ProgressActionEnabled);
            Assert.Equal(100, sut.State.ProgressValue);
            Assert.False(sut.State.ProgressIndeterminate);
            sut.CloseDialog();
        });
    }

    [Fact]
    public void DisplayErrorMessage_MakesErrorStateTerminal()
    {
        RunInSta(() =>
        {
            EnsureAppResources();
            var sut = new AutoUpdateDialogWindow
            {
                SuppressPresentation = true
            };

            sut.ShowDownloadProgress(new AppCastItem
            {
                ShortVersion = "1.0.0.51",
                Version = "1.0.0.51",
                DownloadLink = "https://example.invalid/TOTP-Manager-fast.zip",
                UpdateSize = 54239853
            });

            sut.OnDownloadProgressChanged(new ItemDownloadProgressEventArgs(52, this, 28637006, 54239853));
            sut.DisplayErrorMessage("The response ended prematurely.");
            sut.OnDownloadProgressChanged(new ItemDownloadProgressEventArgs(99, this, 50000000, 54239853));
            sut.FinishedDownloadingFile(true);

            Assert.Equal("Download interrupted", sut.State.ProgressTitleText);
            Assert.Equal("Error", sut.State.ProgressStateText);
            Assert.Equal("The update package could not be downloaded completely.", sut.State.ProgressDescriptionText);
            Assert.Equal("The response ended prematurely.", sut.State.ProgressErrorText);
            Assert.True(sut.State.ProgressErrorVisible);
            Assert.Equal("Close", sut.State.ProgressActionText);
            Assert.True(sut.State.ProgressActionEnabled);
            Assert.Equal(52, sut.State.ProgressValue);
            Assert.False(sut.State.ProgressIndeterminate);
            sut.CloseDialog();
        });
    }

    [Fact]
    public void SetDownloadAndInstallButtonEnabled_DoesNotDisableButtonAfterReadyState()
    {
        RunInSta(() =>
        {
            EnsureAppResources();
            var sut = new AutoUpdateDialogWindow
            {
                SuppressPresentation = true
            };

            sut.ShowDownloadProgress(new AppCastItem
            {
                ShortVersion = "1.0.0.51",
                Version = "1.0.0.51",
                DownloadLink = "https://example.invalid/TOTP-Manager-fast.zip",
                UpdateSize = 54239853
            });

            sut.FinishedDownloadingFile(true);

            Thread.Sleep(1000);
            DispatcherUtil.DoEvents();

            sut.SetDownloadAndInstallButtonEnabled(false);

            Assert.True(sut.State.ProgressActionEnabled);
            Assert.Equal("Install update", sut.State.ProgressActionText);
            sut.CloseDialog();
        });
    }

    [Fact]
    public void ShowUpdateAvailable_ThenInstall_KeepsSingleDialogAndSwitchesToProgressState()
    {
        RunInSta(() =>
        {
            EnsureAppResources();
            var sut = new AutoUpdateDialogWindow
            {
                SuppressPresentation = true
            };
            var responseCount = 0;
            sut.UpdateResponseHandler = (_, _) => responseCount++;

            sut.ShowUpdateAvailable(
                [
                    new AppCastItem
                    {
                        Title = "TOTP Manager",
                        ShortVersion = "1.0.0.51",
                        Version = "1.0.0.51",
                        DownloadLink = "https://example.invalid/TOTP-Manager-fast.zip",
                        UpdateSize = 54239853
                    }
                ],
                isUpdateAlreadyDownloaded: false,
                hideReleaseNotes: false,
                hideRemindMeLaterButton: false,
                hideSkipButton: false);

            sut.State.InstallCommand.Execute(null);
            DispatcherUtil.DoEvents();

            Assert.Equal(1, responseCount);
            Assert.Equal(AutoUpdateDialogStep.Progress, sut.State.CurrentStep);
            Assert.True(sut.State.IsProgressVisible);
            Assert.False(sut.State.IsAvailableVisible);
            Assert.False(sut.IsVisible);
            sut.CloseDialog();
        });
    }

    [Fact]
    public void ShowUpdateAvailable_UsesOwnerCenteredMessagingAndRecommendedOffer()
    {
        RunInSta(() =>
        {
            EnsureAppResources();
            var sut = new AutoUpdateDialogWindow
            {
                SuppressPresentation = true
            };

            sut.ShowUpdateAvailable(
                [
                    new AppCastItem
                    {
                        Title = "TOTP Manager Windows Package",
                        ShortVersion = "1.2.0",
                        Version = "1.2.0",
                        AppVersionInstalled = "1.1.0",
                        DownloadLink = "https://downloads.example.invalid/TOTP-Manager-win.zip",
                        UpdateSize = 54239853
                    },
                    new AppCastItem
                    {
                        Title = "TOTP Manager Portable Package",
                        ShortVersion = "1.2.0",
                        Version = "1.2.0",
                        AppVersionInstalled = "1.1.0",
                        DownloadLink = "https://downloads.example.invalid/TOTP-Manager-portable.zip",
                        UpdateSize = 1234
                    }
                ],
                isUpdateAlreadyDownloaded: false,
                hideReleaseNotes: false,
                hideRemindMeLaterButton: false,
                hideSkipButton: false);

            Assert.Equal("TOTP Manager update ready", sut.State.AvailableHeaderText);
            Assert.Equal("Verified", sut.State.AvailableStatusText);
            Assert.Equal("Current installation: 1.1.0", sut.State.InstalledVersionText);
            Assert.Equal("Recommended release: 1.2.0", sut.State.CurrentVersionText);
            Assert.Equal("Signed package source: downloads.example.invalid", sut.State.AvailableTrustText);
            Assert.Equal("This is the primary release option for the current installation.", sut.State.AvailableRecommendationText);
            Assert.Equal(2, sut.State.Updates.Count);
            Assert.True(sut.State.Updates[0].IsRecommended);
            Assert.False(sut.State.Updates[1].IsRecommended);
            sut.CloseDialog();
        });
    }

    private static void EnsureAppResources()
    {
        TestBootstrap.RegisterSyncfusionLicense();

        if (Application.Current != null)
        {
            return;
        }

        var application = new Application
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown
        };
        application.Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new System.Uri("/TOTP.UI.WPF;component/Styles/Theme.xaml", System.UriKind.Relative)
        });
        application.Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new System.Uri("/TOTP.UI.WPF;component/Styles/Common.xaml", System.UriKind.Relative)
        });
    }

    private static void RunInSta(Action action)
    {
        Exception? captured = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                captured = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (captured != null)
        {
            throw new XunitException(captured.ToString());
        }
    }
}
