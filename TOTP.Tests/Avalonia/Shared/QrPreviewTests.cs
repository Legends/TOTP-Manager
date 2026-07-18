using Avalonia.Media;
using Moq;
using TOTP.Avalonia.Shared.Controls;

namespace TOTP.Tests.Avalonia.Shared;

public sealed class QrPreviewTests
{
    [Fact]
    public void Defaults_AreHiddenAndTreatQrAsSensitive()
    {
        var sut = new QrPreview();

        Assert.False(sut.IsVisible);
        Assert.Contains("account secret", sut.PrivacyNotice, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Source_ControlsVisibilityWithoutAParallelBinding()
    {
        var sut = new QrPreview();

        sut.Source = Mock.Of<IImage>();
        Assert.True(sut.IsVisible);

        sut.Source = null;
        Assert.False(sut.IsVisible);
    }
}
