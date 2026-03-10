using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetSparkleUpdater;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.SignatureVerifiers;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using TOTP.Services.Interfaces;

namespace TOTP.Services;

public sealed class AutoUpdateService : IAutoUpdateService
{
    private static readonly bool EnableDiagnostics = true;
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
                RelaunchAfterUpdate = true
            };

            if (EnableDiagnostics)
            {
                await LogUpdateInfoAsync("quiet check", await _sparkle.CheckForUpdatesQuietly());
            }

            _sparkle.StartLoop(checkOnStartup);
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
            "Auto-update diagnostics: appcast fetch status={StatusCode} content_length={ContentLength}",
            (int)response.StatusCode,
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
