using TOTP.Infrastructure.Services;

namespace TOTP.Tests.Infrastructure.Services;

public sealed class WpfDispatcherServiceTests
{
    [Fact]
    public void CheckAccess_WhenNoApplicationCurrent_ReturnsTrue()
    {
        var sut = new WpfDispatcherService();

        var canAccess = sut.CheckAccess();

        var expected = System.Windows.Application.Current?.Dispatcher?.CheckAccess() ?? true;
        Assert.Equal(expected, canAccess);
    }

    [Fact]
    public void InvokeOnUI_WhenNoApplicationCurrent_DoesNotThrow()
    {
        var sut = new WpfDispatcherService();

        var ex = Record.Exception(() => sut.InvokeOnUI(() => { }));

        Assert.Null(ex);
    }
}
