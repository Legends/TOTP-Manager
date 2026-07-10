using TOTP.Core.Common;
using TOTP.Services;
using TOTP.Tests.Common;
using TOTP.Xaml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace TOTP.Tests.Services;

[Collection(NonParallelCollectionDefinition.NonParallel)]
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

    [StaFact]
    public void ResxExtension_HandlesLanguageChangedFromBackgroundThread()
    {
        File.WriteAllText(_settingsPath, "{}");

        var textBlock = new TextBlock();
        var extension = new ResxExtension { Key = "ui_btnAdd" };
        var provider = new ProvideValueTargetService(textBlock, TextBlock.TextProperty);

        textBlock.Text = (string)extension.ProvideValue(provider);

        Task.Run(() => LocalizationService.ChangeCulture("de-DE")).GetAwaiter().GetResult();

        Assert.False(string.IsNullOrWhiteSpace(textBlock.Text));

        textBlock.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
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

    private sealed class ProvideValueTargetService : IServiceProvider, IProvideValueTarget
    {
        public ProvideValueTargetService(object targetObject, object targetProperty)
        {
            TargetObject = targetObject;
            TargetProperty = targetProperty;
        }

        public object TargetObject { get; }

        public object TargetProperty { get; }

        public object? GetService(Type serviceType)
        {
            return serviceType == typeof(IProvideValueTarget) ? this : null;
        }
    }
}
