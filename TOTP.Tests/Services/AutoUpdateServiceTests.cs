using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NetSparkleUpdater;
using NetSparkleUpdater.Configurations;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.Events;
using NetSparkleUpdater.Interfaces;
using Moq;
using System.ComponentModel;
using System.Windows;
using TOTP.AutoUpdate;
using TOTP.Tests.Common;
using TOTP.Services;
using TOTP.Infrastructure.Platform;
using TOTP.Core.Services.Interfaces;

namespace TOTP.Tests.Services;

[Collection(NonParallelCollectionDefinition.NonParallel)]
public sealed class AutoUpdateServiceTests
{
    private readonly WindowsApplicationPaths _applicationPaths = new(
        AppContext.BaseDirectory,
        Path.Combine(Path.GetTempPath(), $"totp-update-tests-{Guid.NewGuid():N}"));
    private readonly Mock<IUiScheduler> _uiScheduler = new();
    private readonly Mock<IApplicationLifetime> _applicationLifetime = new();

    public AutoUpdateServiceTests()
    {
        _uiScheduler.Setup(s => s.InvokeAsync(It.IsAny<Action>()))
            .Returns<Action>(action =>
            {
                action();
                return Task.CompletedTask;
            });
        _uiScheduler.Setup(s => s.InvokeAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(action => action());
    }

    [Fact]
    public async Task InitializeAsync_WhenAutoUpdateDisabled_DoesNotCreateRuntime()
    {
        var factory = new FakeAutoUpdateRuntimeFactory();
        var sut = CreateSut(BuildConfiguration(("AutoUpdate:Enabled", "false")), factory);

        await sut.InitializeAsync();

        Assert.Equal(0, factory.CreateCount);
    }

    [Fact]
    public void CheckForUpdatesInteractiveAsync_ShowsAndClosesCheckingUi_AndShowsUpToDateWhenNoUpdateExists()
    {
        RunInStaAsync(async () =>
        {
            EnsureApplication();
            var client = new FakeAutoUpdateClient
            {
                QuietResult = null
            };
            var ui = new FakeAutoUpdateUiCoordinator();
            var factory = new FakeAutoUpdateRuntimeFactory(client, ui);
            var sut = CreateSut(BuildEnabledConfiguration(), factory);

            await sut.CheckForUpdatesInteractiveAsync();

            Assert.Equal(1, ui.CheckingWindow.ShowCount);
            Assert.Equal(1, ui.CheckingWindow.CloseCount);
            Assert.Equal(1, ui.ShowVersionIsUpToDateCount);
            Assert.Contains(client.QuietCheckCalls, call => call == true);
        });
    }

    [Fact]
    public async Task CheckForUpdatesInteractiveAsync_ClosesCheckingUi_WhenQuietCheckThrows()
    {
        var client = new FakeAutoUpdateClient
        {
            QuietException = new InvalidOperationException("boom")
        };
        var ui = new FakeAutoUpdateUiCoordinator();
        var factory = new FakeAutoUpdateRuntimeFactory(client, ui);
        var sut = CreateSut(BuildEnabledConfiguration(), factory);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.CheckForUpdatesInteractiveAsync());

        Assert.Equal(1, ui.CheckingWindow.ShowCount);
        Assert.Equal(1, ui.CheckingWindow.CloseCount);
    }

    [Fact]
    public void CheckForUpdatesInteractiveAsync_DoesNotRecreateRuntimeAfterFirstInitialization()
    {
        RunInStaAsync(async () =>
        {
            EnsureApplication();
            var factory = new FakeAutoUpdateRuntimeFactory(new FakeAutoUpdateClient(), new FakeAutoUpdateUiCoordinator());
            var sut = CreateSut(BuildEnabledConfiguration(), factory);

            await sut.CheckForUpdatesInteractiveAsync();
            await sut.CheckForUpdatesInteractiveAsync();

            Assert.Equal(1, factory.CreateCount);
        });
    }

    [Fact]
    public async Task Dispose_AfterInitialization_StopsAndDisposesUpdater()
    {
        var client = new FakeAutoUpdateClient();
        var factory = new FakeAutoUpdateRuntimeFactory(client, new FakeAutoUpdateUiCoordinator());
        var sut = CreateSut(BuildEnabledConfiguration(), factory);

        await sut.InitializeAsync();

        sut.Dispose();

        Assert.Equal(1, client.StopLoopCount);
        Assert.Equal(1, client.DisposeCount);
    }

    [Fact]
    public async Task CloseApplicationEvent_UsesApplicationLifetimeBoundary()
    {
        var client = new FakeAutoUpdateClient();
        var factory = new FakeAutoUpdateRuntimeFactory(client, new FakeAutoUpdateUiCoordinator());
        var sut = CreateSut(BuildEnabledConfiguration(), factory);
        await sut.InitializeAsync();

        Assert.Equal(1, factory.CreateCount);
        Assert.True(client.HasCloseApplicationHandler);
        client.RaiseCloseApplication();

        _applicationLifetime.Verify(l => l.Shutdown(0), Times.Once);
    }

    private AutoUpdateService CreateSut(IConfiguration configuration, FakeAutoUpdateRuntimeFactory factory)
    {
        return new AutoUpdateService(
            configuration,
            NullLogger<AutoUpdateService>.Instance,
            NullLoggerFactory.Instance,
            _applicationPaths,
            _uiScheduler.Object,
            _applicationLifetime.Object,
            factory,
            _ => Task.CompletedTask,
            () => typeof(AutoUpdateService).Assembly.Location);
    }

    private static IConfiguration BuildEnabledConfiguration()
    {
        return BuildConfiguration(
            ("AutoUpdate:Enabled", "true"),
            ("AutoUpdate:AppcastUrl", "https://example.invalid/appcast.xml"),
            ("AutoUpdate:PublicKey", "not-used-in-tests"),
            ("AutoUpdate:CheckOnStartup", "false"));
    }

    private static IConfiguration BuildConfiguration(params (string Key, string Value)[] entries)
    {
        var initialData = entries.ToDictionary(entry => entry.Key, entry => (string?)entry.Value);
        return new ConfigurationBuilder()
            .AddInMemoryCollection(initialData)
            .Build();
    }

    private static void EnsureApplication()
    {
        if (Application.Current != null)
        {
            return;
        }

        _ = new Application
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown
        };
    }

    private static void RunInSta(Action testBody)
    {
        WpfTestHost.Run(testBody);
    }

    private static void RunInStaAsync(Func<Task> testBody)
    {
        WpfTestHost.RunAsync(testBody);
    }

    private sealed class FakeAutoUpdateRuntimeFactory : IAutoUpdateRuntimeFactory
    {
        private readonly FakeAutoUpdateClient _client;
        private readonly FakeAutoUpdateUiCoordinator _ui;

        public FakeAutoUpdateRuntimeFactory()
            : this(new FakeAutoUpdateClient(), new FakeAutoUpdateUiCoordinator())
        {
        }

        public FakeAutoUpdateRuntimeFactory(FakeAutoUpdateClient client, FakeAutoUpdateUiCoordinator ui)
        {
            _client = client;
            _ui = ui;
        }

        public int CreateCount { get; private set; }

        public IAutoUpdateRuntime Create(
            string appcastUrl,
            string publicKey,
            Func<AppCastItem, string?, Task<bool>> customInstallHandler,
            ILogger<AutoUpdateDialogWindow> progressWindowLogger)
        {
            CreateCount++;
            return new NetSparkleAutoUpdateRuntime(_client, _ui);
        }
    }

    private sealed class FakeAutoUpdateClient : IAutoUpdateClient
    {
        private Action? _loopStarted;
        private Action<bool>? _loopFinished;
        private Action? _updateCheckStarted;
        private Action<UpdateStatus>? _updateCheckFinished;
        private Action<object?, UpdateDetectedEventArgs>? _updateDetected;
        private Action<UpdateResponseEventArgs>? _userResponded;
        private Action<AppCastItem, string?>? _downloadStarted;
        private Action<AppCastItem, string?>? _downloadCanceled;
        private Action<object?, AppCastItem?, ItemDownloadProgressEventArgs?>? _downloadMadeProgress;
        private Action<AppCastItem, string?>? _downloadFinished;
        private Action<AppCastItem, string?>? _downloadedFileIsCorrupt;
        private Action<AppCastItem, string?>? _downloadedFileThrewWhileCheckingSignature;
        private Action<AppCastItem, string?, Exception>? _downloadHadError;
        private Action<CancelEventArgs>? _preparingToExit;
        private Action? _closeApplication;

        public Configuration? Configuration { get; set; }
        public UpdateInfo? QuietResult { get; set; }
        public Exception? QuietException { get; set; }
        public List<bool> QuietCheckCalls { get; } = [];
        public int StopLoopCount { get; private set; }
        public int DisposeCount { get; private set; }
        public bool HasCloseApplicationHandler => _closeApplication != null;

        public Task<UpdateInfo?> CheckForUpdatesQuietly(bool userInitiated)
        {
            QuietCheckCalls.Add(userInitiated);
            if (QuietException != null)
            {
                throw QuietException;
            }

            return Task.FromResult(QuietResult);
        }

        public Task<UpdateInfo?> CheckForUpdatesAtUserRequest(bool userInitiated) => Task.FromResult<UpdateInfo?>(null);
        public void StartLoop(bool checkOnStartup, bool forceStartupCheck, TimeSpan loopInterval)
        {
        }

        public void StopLoop()
        {
            StopLoopCount++;
        }

        public void ShowUpdateNeededUI(List<AppCastItem> updates, bool isUpdateAlreadyDownloaded)
        {
        }
        public void OnLoopStarted(Action callback) => _loopStarted = callback;
        public void OnLoopFinished(Action<bool> callback) => _loopFinished = callback;
        public void OnUpdateCheckStarted(Action callback) => _updateCheckStarted = callback;
        public void OnUpdateCheckFinished(Action<UpdateStatus> callback) => _updateCheckFinished = callback;
        public void OnUpdateDetected(Action<object?, UpdateDetectedEventArgs> callback) => _updateDetected = callback;
        public void OnUserRespondedToUpdate(Action<UpdateResponseEventArgs> callback) => _userResponded = callback;
        public void OnDownloadStarted(Action<AppCastItem, string?> callback) => _downloadStarted = callback;
        public void OnDownloadCanceled(Action<AppCastItem, string?> callback) => _downloadCanceled = callback;
        public void OnDownloadMadeProgress(Action<object?, AppCastItem?, ItemDownloadProgressEventArgs?> callback) => _downloadMadeProgress = callback;
        public void OnDownloadFinished(Action<AppCastItem, string?> callback) => _downloadFinished = callback;
        public void OnDownloadedFileIsCorrupt(Action<AppCastItem, string?> callback) => _downloadedFileIsCorrupt = callback;
        public void OnDownloadedFileThrewWhileCheckingSignature(Action<AppCastItem, string?> callback) => _downloadedFileThrewWhileCheckingSignature = callback;
        public void OnDownloadHadError(Action<AppCastItem, string?, Exception> callback) => _downloadHadError = callback;
        public void OnPreparingToExit(Action<CancelEventArgs> callback) => _preparingToExit = callback;
        public void OnCloseApplication(Action callback) => _closeApplication = callback;
        public void RaiseCloseApplication() => _closeApplication?.Invoke();
        public void Dispose() => DisposeCount++;
    }

    private sealed class FakeAutoUpdateUiCoordinator : IAutoUpdateUiCoordinator
    {
        public FakeCheckingForUpdates CheckingWindow { get; } = new();
        public int ShowVersionIsUpToDateCount { get; private set; }
        public List<(AppCastItem Item, string? Path)> DownloadStarted { get; } = [];
        public List<(AppCastItem Item, string? Path)> DownloadFinished { get; } = [];

        public ICheckingForUpdates ShowCheckingForUpdates() => CheckingWindow;

        public void ShowVersionIsUpToDate()
        {
            ShowVersionIsUpToDateCount++;
        }

        public void NotifyDownloadStarted(AppCastItem item, string? path)
        {
            DownloadStarted.Add((item, path));
        }

        public void SetDownloadedFilePath(AppCastItem item, string? downloadedFilePath)
        {
            DownloadFinished.Add((item, downloadedFilePath));
        }
    }

    private sealed class FakeCheckingForUpdates : ICheckingForUpdates
    {
        public event EventHandler? UpdatesUIClosing;
        public int ShowCount { get; private set; }
        public int CloseCount { get; private set; }

        public void Show()
        {
            ShowCount++;
        }

        public void Close()
        {
            CloseCount++;
            UpdatesUIClosing?.Invoke(this, EventArgs.Empty);
        }
    }
}
