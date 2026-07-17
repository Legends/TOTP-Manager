using Microsoft.Extensions.Logging.Abstractions;
using TOTP.Core.Services.Models;
using TOTP.Presentation.Platform;

namespace TOTP.Tests.Presentation.Platform;

public sealed class WpfClipboardTests
{
    [Fact]
    public void Capabilities_AdvertiseWriteAndConditionalClear()
    {
        var sut = new WpfClipboard(NullLogger<WpfClipboard>.Instance);

        Assert.True(sut.Capabilities.HasFlag(ClipboardCapabilities.WriteText));
        Assert.True(sut.Capabilities.HasFlag(ClipboardCapabilities.ConditionalClear));
    }
}
