using NetSparkleUpdater;
using System.Threading;
using System.Windows;
using TOTP.AutoUpdate;

namespace TOTP.Tests.AutoUpdate;

public sealed class TOTPNetSparkleUiFactoryTests
{
    [Fact]
    public void CreateUpdateAvailableWindow_WhenDownloadedFileWasRemembered_ShowsInstallReadyState()
    {
        RunInSta(() =>
        {
            EnsureApplication();
            var dialog = new AutoUpdateDialogWindow
            {
                SuppressPresentation = true
            };
            var sut = new TOTPNetSparkleUiFactory(
                dialogFactory: () => dialog);
            var item = CreateItem();
            var tempFile = Path.GetTempFileName();

            try
            {
                sut.SetDownloadedFilePath(item, tempFile);

                var controller = sut.CreateUpdateAvailableWindow(null!, [item], isUpdateAlreadyDownloaded: false);
                controller.Show(true);

                Assert.True(dialog.State.IsDownloadReady);
                Assert.Equal("Install update", dialog.State.InstallButtonText);
                dialog.CloseDialog();
            }
            finally
            {
                File.Delete(tempFile);
            }
        });
    }

    [Fact]
    public void UnifiedDownloadProgress_ActionDuringActiveDownload_CancelsUnderlyingTransfer()
    {
        RunInSta(() =>
        {
            EnsureApplication();
            var dialog = new AutoUpdateDialogWindow
            {
                SuppressPresentation = true
            };
            var controller = new FakeDownloadController();
            var sut = new UnifiedDownloadProgress(dialog, controller, CreateItem(), null, null, TimeSpan.FromSeconds(1));

            sut.Show(isOnMainThread: true);

            dialog.ProgressActionHandler!().GetAwaiter().GetResult();

            Assert.Equal(1, controller.CancelCount);
        });
    }

    [Fact]
    public void UnifiedDownloadProgress_WhenDownloadNeverStarts_ShowsTimeoutError()
    {
        RunInSta(() =>
        {
            EnsureApplication();
            var dialog = new AutoUpdateDialogWindow
            {
                SuppressPresentation = true
            };
            var sut = new UnifiedDownloadProgress(
                dialog,
                new FakeDownloadController(),
                CreateItem(),
                null,
                null,
                TimeSpan.FromMilliseconds(75));

            sut.Show(isOnMainThread: true);

            Thread.Sleep(180);
            DispatcherUtil.DoEvents();

            Assert.Equal("Error", dialog.State.ProgressStateText);
            Assert.Equal("Download interrupted", dialog.State.ProgressTitleText);
            Assert.True(dialog.State.ProgressErrorVisible);
            dialog.CloseDialog();
        });
    }

    private static AppCastItem CreateItem()
    {
        return new AppCastItem
        {
            Title = "TOTP Manager",
            ShortVersion = "1.0.0",
            Version = "1.0.0",
            AppVersionInstalled = "0.9.0",
            DownloadLink = "https://example.invalid/TOTP-Manager.zip",
            UpdateSize = 1234
        };
    }

    private static void EnsureApplication()
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
            Source = new Uri("/TOTP.UI.WPF;component/Styles/Theme.xaml", UriKind.Relative)
        });
        application.Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri("/TOTP.UI.WPF;component/Styles/Common.xaml", UriKind.Relative)
        });
    }

    private static void RunInSta(Action testBody)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                testBody();
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (failure != null)
        {
            throw failure;
        }
    }

    private sealed class FakeDownloadController : IUpdateDownloadController
    {
        public int CancelCount { get; private set; }

        public void CancelFileDownload()
        {
            CancelCount++;
        }
    }
}
