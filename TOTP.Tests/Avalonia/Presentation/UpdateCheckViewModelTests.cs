using TOTP.Avalonia.Desktop.Presentation;
using TOTP.Infrastructure.Services;

namespace TOTP.Tests.Avalonia.Presentation;

public sealed class UpdateCheckViewModelTests
{
    [Fact]
    public async Task CheckAsync_VerifiesSignedFixtureAndDoesNotStartDownload()
    {
        var sut = new UpdateCheckViewModel(new SignedAppcastVerifier());

        await sut.CheckAsync();

        Assert.Contains("accepted", sut.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("No download", sut.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("example.invalid", sut.Message, StringComparison.OrdinalIgnoreCase);
    }
}
