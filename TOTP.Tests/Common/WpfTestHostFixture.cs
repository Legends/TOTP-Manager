using Xunit;

[assembly: AssemblyFixture(typeof(TOTP.Tests.Common.WpfTestHostFixture))]

namespace TOTP.Tests.Common;

public sealed class WpfTestHostFixture : IDisposable
{
    public void Dispose()
    {
        WpfTestHost.Shutdown();
    }
}
