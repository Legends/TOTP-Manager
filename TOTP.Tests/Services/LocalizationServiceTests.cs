using TOTP.Infrastructure.Common;
using TOTP.Services;

namespace TOTP.Tests.Services;

public sealed class LocalizationServiceTests : IDisposable
{
    private readonly string _settingsPath = StringsConstants.AppSettingsJsonFilePath;
    private readonly string? _originalContent;

    public LocalizationServiceTests()
    {
        _originalContent = File.Exists(_settingsPath) ? File.ReadAllText(_settingsPath) : null;
    }

    [Fact]
    public void ChangeCulture_UpdatesThreadCulture_AndWritesLocalizationSetting_AndRaisesEvent()
    {
        File.WriteAllText(_settingsPath, "{}");
        var raised = false;
        void Handler() => raised = true;
        LocalizationService.LanguageChanged += Handler;

        try
        {
            LocalizationService.ChangeCulture("de-DE");
        }
        finally
        {
            LocalizationService.LanguageChanged -= Handler;
        }

        Assert.True(raised);
        Assert.Equal("de-DE", Thread.CurrentThread.CurrentCulture.Name);
        Assert.Equal("de-DE", Thread.CurrentThread.CurrentUICulture.Name);

        var json = File.ReadAllText(_settingsPath);
        Assert.Contains("\"Localization\"", json);
        Assert.Contains("\"Culture\": \"de-DE\"", json);
    }

    [Fact]
    public void ChangeCulture_WhenLocalizationExists_UpdatesOnlyCultureValue()
    {
        File.WriteAllText(_settingsPath, """{"Localization":{"Culture":"en-US"},"Other":{"A":1}}""");

        LocalizationService.ChangeCulture("fr-FR");

        var json = File.ReadAllText(_settingsPath);
        Assert.Contains("\"Culture\": \"fr-FR\"", json);
        Assert.Contains("\"Other\"", json);
    }

    public void Dispose()
    {
        try
        {
            if (_originalContent is null)
            {
                if (File.Exists(_settingsPath))
                {
                    File.Delete(_settingsPath);
                }
            }
            else
            {
                File.WriteAllText(_settingsPath, _originalContent);
            }
        }
        catch
        {
            // best-effort cleanup
        }
    }
}
