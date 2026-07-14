using FluentResults;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using TOTP.Core.Models;
using TOTP.Core.Security.Interfaces;
using TOTP.Services;
using TOTP.Tests.Common;
using TOTP.Xaml;

namespace TOTP.Tests.Services;

[Collection(NonParallelCollectionDefinition.NonParallel)]
public sealed class LocalizationServiceTests : IDisposable
{
    private readonly CultureInfo _originalCulture = CultureInfo.CurrentCulture;
    private readonly CultureInfo _originalUiCulture = CultureInfo.CurrentUICulture;

    [Fact]
    public void ChangeCulture_UpdatesCultureAndSetting_AndRaisesEvent()
    {
        var settings = new AppSettings();
        var settingsService = CreateSettingsService(settings);
        var sut = new LocalizationService(
            settingsService.Object,
            NullLogger<LocalizationService>.Instance);
        var raised = false;
        sut.LanguageChanged += () => raised = true;

        sut.ChangeCulture("de-DE");

        Assert.True(raised);
        Assert.Equal("de-DE", Thread.CurrentThread.CurrentCulture.Name);
        Assert.Equal("de-DE", Thread.CurrentThread.CurrentUICulture.Name);
        Assert.Equal("de-DE", settings.CultureName);
        settingsService.Verify(x => x.SaveAsync(), Times.Once);
    }

    [Fact]
    public void ApplyCurrentCulture_UsesPersistedSettingWithoutSavingAgain()
    {
        var settings = new AppSettings { CultureName = "fr-FR" };
        var settingsService = CreateSettingsService(settings);
        var sut = new LocalizationService(
            settingsService.Object,
            NullLogger<LocalizationService>.Instance);

        sut.ApplyCurrentCulture();

        Assert.Equal("fr-FR", CultureInfo.CurrentUICulture.Name);
        settingsService.Verify(x => x.SaveAsync(), Times.Never);
    }

    [StaFact]
    public void ResxExtension_HandlesLanguageChangedFromBackgroundThread()
    {
        var settings = new AppSettings();
        var settingsService = CreateSettingsService(settings);
        var sut = new LocalizationService(
            settingsService.Object,
            NullLogger<LocalizationService>.Instance);
        var textBlock = new TextBlock();
        var extension = new ResxExtension { Key = "ui_btnAdd" };
        var provider = new ProvideValueTargetService(textBlock, TextBlock.TextProperty);

        textBlock.Text = (string)extension.ProvideValue(provider);
        Task.Run(() => sut.ChangeCulture("de-DE")).GetAwaiter().GetResult();

        Assert.False(string.IsNullOrWhiteSpace(textBlock.Text));
        textBlock.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
    }

    public void Dispose()
    {
        CultureInfo.CurrentCulture = _originalCulture;
        CultureInfo.CurrentUICulture = _originalUiCulture;
        CultureInfo.DefaultThreadCurrentCulture = _originalCulture;
        CultureInfo.DefaultThreadCurrentUICulture = _originalUiCulture;
    }

    private static Mock<ISettingsService> CreateSettingsService(IAppSettings settings)
    {
        var mock = new Mock<ISettingsService>();
        mock.SetupGet(x => x.Current).Returns(settings);
        mock.Setup(x => x.SaveAsync()).ReturnsAsync(Result.Ok());
        return mock;
    }

    private sealed class ProvideValueTargetService(
        object targetObject,
        object targetProperty) : IServiceProvider, IProvideValueTarget
    {
        public object TargetObject { get; } = targetObject;
        public object TargetProperty { get; } = targetProperty;

        public object? GetService(Type serviceType)
            => serviceType == typeof(IProvideValueTarget) ? this : null;
    }
}
