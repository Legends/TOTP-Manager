using Microsoft.Extensions.Logging;
using NetSparkleUpdater;
using NetSparkleUpdater.Configurations;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.Events;
using NetSparkleUpdater.Interfaces;
using NetSparkleUpdater.SignatureVerifiers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using TOTP.AutoUpdate;

namespace TOTP.Services;

internal interface IAutoUpdateRuntimeFactory
{
    IAutoUpdateRuntime Create(
        string appcastUrl,
        string publicKey,
        Func<AppCastItem, string?, Task<bool>> customInstallHandler,
        ILogger<AutoUpdateDialogWindow> progressWindowLogger);
}

internal interface IAutoUpdateRuntime
{
    IAutoUpdateClient Client { get; }
    IAutoUpdateUiCoordinator Ui { get; }
}

internal interface IAutoUpdateClient
{
    Configuration? Configuration { get; set; }

    Task<UpdateInfo?> CheckForUpdatesQuietly(bool userInitiated);
    Task<UpdateInfo?> CheckForUpdatesAtUserRequest(bool userInitiated);
    void StartLoop(bool checkOnStartup, bool forceStartupCheck, TimeSpan loopInterval);
    void ShowUpdateNeededUI(List<AppCastItem> updates, bool isUpdateAlreadyDownloaded);
    void OnLoopStarted(Action callback);
    void OnLoopFinished(Action<bool> callback);
    void OnUpdateCheckStarted(Action callback);
    void OnUpdateCheckFinished(Action<UpdateStatus> callback);
    void OnUpdateDetected(Action<object?, UpdateDetectedEventArgs> callback);
    void OnUserRespondedToUpdate(Action<UpdateResponseEventArgs> callback);
    void OnDownloadStarted(Action<AppCastItem, string?> callback);
    void OnDownloadCanceled(Action<AppCastItem, string?> callback);
    void OnDownloadMadeProgress(Action<object?, AppCastItem?, ItemDownloadProgressEventArgs?> callback);
    void OnDownloadFinished(Action<AppCastItem, string?> callback);
    void OnDownloadedFileIsCorrupt(Action<AppCastItem, string?> callback);
    void OnDownloadedFileThrewWhileCheckingSignature(Action<AppCastItem, string?> callback);
    void OnDownloadHadError(Action<AppCastItem, string?, Exception> callback);
    void OnPreparingToExit(Action<CancelEventArgs> callback);
    void OnCloseApplication(Action callback);
}

internal interface IAutoUpdateUiCoordinator
{
    ICheckingForUpdates ShowCheckingForUpdates();
    void ShowVersionIsUpToDate();
    void NotifyDownloadStarted(AppCastItem item, string? path);
    void SetDownloadedFilePath(AppCastItem item, string? downloadedFilePath);
}

internal sealed class NetSparkleAutoUpdateRuntimeFactory : IAutoUpdateRuntimeFactory
{
    public IAutoUpdateRuntime Create(
        string appcastUrl,
        string publicKey,
        Func<AppCastItem, string?, Task<bool>> customInstallHandler,
        ILogger<AutoUpdateDialogWindow> progressWindowLogger)
    {
        var uiFactory = new TOTPNetSparkleUiFactory(customInstallHandler, progressWindowLogger);
        var sparkle = new SparkleUpdater(appcastUrl, new Ed25519Checker(SecurityMode.Strict, publicKey))
        {
            RelaunchAfterUpdate = false,
            UIFactory = uiFactory
        };

        return new NetSparkleAutoUpdateRuntime(
            new NetSparkleAutoUpdateClient(sparkle),
            new NetSparkleAutoUpdateUiCoordinator(sparkle, uiFactory));
    }
}

internal sealed class NetSparkleAutoUpdateRuntime : IAutoUpdateRuntime
{
    public NetSparkleAutoUpdateRuntime(IAutoUpdateClient client, IAutoUpdateUiCoordinator ui)
    {
        Client = client;
        Ui = ui;
    }

    public IAutoUpdateClient Client { get; }
    public IAutoUpdateUiCoordinator Ui { get; }
}

internal sealed class NetSparkleAutoUpdateClient : IAutoUpdateClient
{
    private readonly SparkleUpdater _sparkle;

    public NetSparkleAutoUpdateClient(SparkleUpdater sparkle)
    {
        _sparkle = sparkle;
    }

    public Configuration? Configuration
    {
        get => _sparkle.Configuration;
        set => _sparkle.Configuration = value;
    }

    public Task<UpdateInfo?> CheckForUpdatesQuietly(bool userInitiated) => _sparkle.CheckForUpdatesQuietly(userInitiated);
    public Task<UpdateInfo?> CheckForUpdatesAtUserRequest(bool userInitiated) => _sparkle.CheckForUpdatesAtUserRequest(userInitiated);
    public void StartLoop(bool checkOnStartup, bool forceStartupCheck, TimeSpan loopInterval) => _sparkle.StartLoop(checkOnStartup, forceStartupCheck, loopInterval);
    public void ShowUpdateNeededUI(List<AppCastItem> updates, bool isUpdateAlreadyDownloaded) => _sparkle.ShowUpdateNeededUI(updates, isUpdateAlreadyDownloaded);
    public void OnLoopStarted(Action callback) => _sparkle.LoopStarted += _ => callback();
    public void OnLoopFinished(Action<bool> callback) => _sparkle.LoopFinished += (_, updateRequired) => callback(updateRequired);
    public void OnUpdateCheckStarted(Action callback) => _sparkle.UpdateCheckStarted += _ => callback();
    public void OnUpdateCheckFinished(Action<UpdateStatus> callback) => _sparkle.UpdateCheckFinished += (_, status) => callback(status);
    public void OnUpdateDetected(Action<object?, UpdateDetectedEventArgs> callback) => _sparkle.UpdateDetected += (sender, args) => callback(sender, args);
    public void OnUserRespondedToUpdate(Action<UpdateResponseEventArgs> callback) => _sparkle.UserRespondedToUpdate += (_, args) => callback(args);
    public void OnDownloadStarted(Action<AppCastItem, string?> callback) => _sparkle.DownloadStarted += (item, path) => callback(item, path);
    public void OnDownloadCanceled(Action<AppCastItem, string?> callback) => _sparkle.DownloadCanceled += (item, path) => callback(item, path);
    public void OnDownloadMadeProgress(Action<object?, AppCastItem?, ItemDownloadProgressEventArgs?> callback) => _sparkle.DownloadMadeProgress += (sender, item, args) => callback(sender, item, args);
    public void OnDownloadFinished(Action<AppCastItem, string?> callback) => _sparkle.DownloadFinished += (item, path) => callback(item, path);
    public void OnDownloadedFileIsCorrupt(Action<AppCastItem, string?> callback) => _sparkle.DownloadedFileIsCorrupt += (item, path) => callback(item, path);
    public void OnDownloadedFileThrewWhileCheckingSignature(Action<AppCastItem, string?> callback) => _sparkle.DownloadedFileThrewWhileCheckingSignature += (item, path) => callback(item, path);
    public void OnDownloadHadError(Action<AppCastItem, string?, Exception> callback) => _sparkle.DownloadHadError += (item, path, exception) => callback(item, path, exception);
    public void OnPreparingToExit(Action<CancelEventArgs> callback) => _sparkle.PreparingToExit += (_, args) => callback(args);
    public void OnCloseApplication(Action callback) => _sparkle.CloseApplication += () => callback();
}

internal sealed class NetSparkleAutoUpdateUiCoordinator : IAutoUpdateUiCoordinator
{
    private readonly SparkleUpdater _sparkle;
    private readonly TOTPNetSparkleUiFactory _uiFactory;

    public NetSparkleAutoUpdateUiCoordinator(SparkleUpdater sparkle, TOTPNetSparkleUiFactory uiFactory)
    {
        _sparkle = sparkle;
        _uiFactory = uiFactory;
    }

    public ICheckingForUpdates ShowCheckingForUpdates()
    {
        return _uiFactory.ShowCheckingForUpdates(_sparkle);
    }

    public void ShowVersionIsUpToDate()
    {
        _uiFactory.ShowVersionIsUpToDate(_sparkle);
    }

    public void NotifyDownloadStarted(AppCastItem item, string? path)
    {
        _uiFactory.NotifyDownloadStarted(item, path);
    }

    public void SetDownloadedFilePath(AppCastItem item, string? downloadedFilePath)
    {
        _uiFactory.SetDownloadedFilePath(item, downloadedFilePath);
    }
}
