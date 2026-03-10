using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetSparkleUpdater;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.Events;
using NetSparkleUpdater.SignatureVerifiers;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using TOTP.AutoUpdate;
using TOTP.Services.Interfaces;

namespace TOTP.Services;

public sealed class AutoUpdateService : IAutoUpdateService
{
    private static readonly bool EnableDiagnostics = true;
    private const int DefaultLoopIntervalHours = 24;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AutoUpdateService> _logger;
    private SparkleUpdater? _sparkle;
    private bool _initialized;

    public AutoUpdateService(IConfiguration configuration, ILogger<AutoUpdateService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        var enabled = _configuration.GetValue("AutoUpdate:Enabled", false);
        if (!enabled)
        {
            _logger.LogInformation("Auto-update disabled by configuration.");
            _initialized = true;
            return;
        }

        var appcastUrl = _configuration["AutoUpdate:AppcastUrl"];
        var publicKey = _configuration["AutoUpdate:PublicKey"];
        var checkOnStartup = _configuration.GetValue("AutoUpdate:CheckOnStartup", true);
        var forceStartupCheck = _configuration.GetValue("AutoUpdate:ForceStartupCheck", false);
        var interactiveDebugCheckOnStartup = _configuration.GetValue("AutoUpdate:InteractiveDebugCheckOnStartup", false);
        var checkIntervalMinutes = _configuration.GetValue<int?>("AutoUpdate:CheckIntervalMinutes");

        if (string.IsNullOrWhiteSpace(appcastUrl) || string.IsNullOrWhiteSpace(publicKey))
        {
            _logger.LogWarning("Auto-update enabled but AppcastUrl/PublicKey is missing.");
            _initialized = true;
            return;
        }

        try
        {
            LogCurrentVersion();
            await LogRemoteAppcastAsync(appcastUrl);

            _sparkle = new SparkleUpdater(
                appcastUrl,
                new Ed25519Checker(SecurityMode.Strict, publicKey))
            {
                RelaunchAfterUpdate = true,
                UIFactory = new TOTPNetSparkleUiFactory()
            };
            WireDiagnostics(_sparkle);

            var effectiveInterval = ResolveCheckInterval(checkIntervalMinutes);
            _logger.LogInformation(
                "Auto-update configuration: check_on_startup={CheckOnStartup} force_startup_check={ForceStartupCheck} interactive_debug_check={InteractiveDebugCheckOnStartup} loop_interval={LoopInterval}",
                checkOnStartup,
                forceStartupCheck,
                interactiveDebugCheckOnStartup,
                effectiveInterval);

            if (EnableDiagnostics)
            {
                await LogUpdateInfoAsync("diagnostic quiet check", await _sparkle.CheckForUpdatesQuietly(true));
            }

            if (interactiveDebugCheckOnStartup)
            {
                _logger.LogWarning(
                    "Auto-update interactive debug check is enabled. NetSparkle will run a user-request style check on startup.");
                var interactiveUpdateInfo = await _sparkle.CheckForUpdatesAtUserRequest(true);
                await LogUpdateInfoAsync("interactive startup check", interactiveUpdateInfo);
                await ShowDebugUiFallbackAsync(interactiveUpdateInfo);
            }

            var shouldDoInitialLoopCheck = checkOnStartup && !interactiveDebugCheckOnStartup;
            _sparkle.StartLoop(shouldDoInitialLoopCheck, forceStartupCheck, effectiveInterval);
            _logger.LogInformation("Auto-update initialized. Appcast={Appcast}", appcastUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Auto-update initialization failed.");
        }
        finally
        {
            _initialized = true;
        }

    }

    public async Task CheckForUpdatesInteractiveAsync()
    {
        if (!_initialized)
        {
            await InitializeAsync();
        }

        if (_sparkle == null)
        {
            _logger.LogWarning("Auto-update interactive check requested, but the updater is not initialized.");
            return;
        }

        _logger.LogInformation("Auto-update manual check requested by the user.");
        var updateInfo = await _sparkle.CheckForUpdatesAtUserRequest(true);
        await LogUpdateInfoAsync("manual interactive check", updateInfo);
    }

    private void WireDiagnostics(SparkleUpdater sparkle)
    {
        sparkle.LoopStarted += _ => _logger.LogInformation("Auto-update event: loop started.");
        sparkle.LoopFinished += (_, updateRequired) =>
            _logger.LogInformation("Auto-update event: loop finished. update_required={UpdateRequired}", updateRequired);
        sparkle.UpdateCheckStarted += _ => _logger.LogInformation("Auto-update event: update check started.");
        sparkle.UpdateCheckFinished += (_, status) =>
            _logger.LogInformation("Auto-update event: update check finished. status={Status}", status);
        sparkle.UpdateDetected += OnUpdateDetected;
        sparkle.UserRespondedToUpdate += (_, args) =>
            _logger.LogInformation("Auto-update event: user responded to update prompt. response={Response}", args.Result);
        sparkle.DownloadStarted += (item, path) =>
            _logger.LogInformation("Auto-update event: download started. version={Version} path={Path}", item.Version, path);
        sparkle.DownloadCanceled += (item, path) =>
            _logger.LogInformation("Auto-update event: download canceled. version={Version} path={Path}", item.Version, path);
        sparkle.DownloadMadeProgress += (_, item, args) =>
            _logger.LogInformation(
                "Auto-update event: download progress. version={Version} downloaded={DownloadedBytes} total={TotalBytes} percentage={Percentage}",
                item?.Version?.ToString() ?? "unknown",
                args?.BytesReceived,
                args?.TotalBytesToReceive,
                args?.ProgressPercentage);
        sparkle.DownloadFinished += (item, path) =>
            _logger.LogInformation("Auto-update event: download finished. version={Version} path={Path}", item.Version, path);
        sparkle.DownloadedFileIsCorrupt += (item, path) =>
            _logger.LogWarning("Auto-update event: downloaded file is corrupt. version={Version} path={Path}", item.Version, path);
        sparkle.DownloadedFileThrewWhileCheckingSignature += (item, path) =>
            _logger.LogWarning("Auto-update event: downloaded file threw while checking signature. version={Version} path={Path}", item.Version, path);
        sparkle.DownloadHadError += (item, path, exception) =>
            _logger.LogWarning(
                exception,
                "Auto-update event: download had an error. version={Version} path={Path}",
                item.Version,
                path);
        sparkle.PreparingToExit += (_, args) =>
        {
            _logger.LogInformation("Auto-update event: preparing to exit. cancel={Cancel}", args.Cancel);
        };
        sparkle.CloseApplication += () =>
        {
            _logger.LogInformation("Auto-update event: close application requested.");
            var application = Application.Current;
            if (application == null)
            {
                _logger.LogWarning("Auto-update close request ignored because there is no current WPF application.");
                return;
            }

            if (application.Dispatcher.CheckAccess())
            {
                application.Shutdown();
                return;
            }

            application.Dispatcher.Invoke(application.Shutdown);
        };
    }

    private void OnUpdateDetected(object? sender, UpdateDetectedEventArgs args)
    {
        var latestVersion = args.LatestVersion;
        var appCastItems = args.AppCastItems?.Count ?? 0;

        _logger.LogInformation(
            "Auto-update event: update detected. latest_version={LatestVersion} latest_short_version={LatestShortVersion} download_url={DownloadUrl} items={ItemCount} next_action={NextAction}",
            latestVersion?.Version,
            latestVersion?.ShortVersion,
            latestVersion?.DownloadLink,
            appCastItems,
            args.NextAction);
    }

    private static TimeSpan ResolveCheckInterval(int? checkIntervalMinutes)
    {
        if (checkIntervalMinutes.HasValue && checkIntervalMinutes.Value > 0)
        {
            return TimeSpan.FromMinutes(checkIntervalMinutes.Value);
        }

        return TimeSpan.FromHours(DefaultLoopIntervalHours);
    }

    private async Task ShowDebugUiFallbackAsync(UpdateInfo? updateInfo)
    {
        if (_sparkle == null || updateInfo?.Status != UpdateStatus.UpdateAvailable)
        {
            return;
        }

        var updates = updateInfo.Updates?.ToList();
        if (updates == null || updates.Count == 0)
        {
            _logger.LogWarning("Auto-update debug UI fallback skipped because there were no update candidates.");
            return;
        }

        if (Application.Current?.Dispatcher == null)
        {
            _logger.LogWarning("Auto-update debug UI fallback skipped because there is no active WPF dispatcher.");
            return;
        }

        _logger.LogWarning(
            "Auto-update debug UI fallback is forcing ShowUpdateNeededUI on the WPF dispatcher. This is only for local diagnosis.");

        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            _sparkle.ShowUpdateNeededUI(updates, true);
        });
    }

    private Task LogUpdateInfoAsync(string label, UpdateInfo? updateInfo)
    {
        if (updateInfo == null)
        {
            _logger.LogInformation("Auto-update diagnostics: {Label} returned null update info.", label);
            return Task.CompletedTask;
        }

        _logger.LogInformation("Auto-update diagnostics: {Label} status={Status}", label, updateInfo.Status);

        var updates = updateInfo.Updates?.ToList();
        _logger.LogInformation("Auto-update diagnostics: {Label} found {Count} update candidate(s).", label, updates?.Count ?? 0);

        if (updates == null)
        {
            return Task.CompletedTask;
        }

        foreach (var update in updates)
        {
            _logger.LogInformation(
                "Auto-update diagnostics: {Label} candidate app_installed={InstalledVersion} candidate_version={Version} candidate_short_version={ShortVersion} url={Url}",
                label,
                update.AppVersionInstalled,
                update.Version,
                update.ShortVersion,
                update.DownloadLink);
        }

        return Task.CompletedTask;
    }

    private void LogCurrentVersion()
    {
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var assemblyVersion = assembly.GetName().Version?.ToString();
        var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        var location = assembly.Location;
        var fileVersion = string.Empty;
        var productVersion = string.Empty;

        if (!string.IsNullOrWhiteSpace(location) && File.Exists(location))
        {
            var fileInfo = FileVersionInfo.GetVersionInfo(location);
            fileVersion = fileInfo.FileVersion ?? string.Empty;
            productVersion = fileInfo.ProductVersion ?? string.Empty;
        }

        _logger.LogInformation(
            "Auto-update diagnostics: installed assembly_version={AssemblyVersion} file_version={FileVersion} product_version={ProductVersion} informational_version={InformationalVersion} location={Location}",
            assemblyVersion,
            fileVersion,
            productVersion,
            informationalVersion,
            location);
    }

    private async Task LogRemoteAppcastAsync(string appcastUrl)
    {
        if (!EnableDiagnostics)
        {
            return;
        }

        using var httpClient = new HttpClient();
        using var response = await httpClient.GetAsync(appcastUrl);
        var content = await response.Content.ReadAsStringAsync();

        _logger.LogInformation(
            "Auto-update diagnostics: appcast fetch status={StatusCode} final_uri={FinalUri} content_length={ContentLength}",
            (int)response.StatusCode,
            response.RequestMessage?.RequestUri?.ToString(),
            content.Length);

        var document = XDocument.Parse(content);
        XNamespace sparkleNs = "http://www.andymatuschak.org/xml-namespaces/sparkle";
        var item = document.Descendants("item").FirstOrDefault();
        var enclosure = item?.Element("enclosure");
        var sparkleVersion = item?.Element(sparkleNs + "version")?.Value;
        var shortVersion = item?.Element(sparkleNs + "shortVersionString")?.Value;
        var enclosureVersion = enclosure?.Attribute(sparkleNs + "version")?.Value;
        var enclosureShortVersion = enclosure?.Attribute(sparkleNs + "shortVersionString")?.Value;
        var enclosureUrl = enclosure?.Attribute("url")?.Value;

        _logger.LogInformation(
            "Auto-update diagnostics: appcast item_version={ItemVersion} item_short_version={ItemShortVersion} enclosure_version={EnclosureVersion} enclosure_short_version={EnclosureShortVersion} enclosure_url={EnclosureUrl}",
            sparkleVersion,
            shortVersion,
            enclosureVersion,
            enclosureShortVersion,
            enclosureUrl);
    }
}
