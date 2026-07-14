using TOTP.Presentation.Services;

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
    public void InvokeOnUI_WhenNoApplicationCurrent_ExecutesActionInline()
    {
        var sut = new WpfDispatcherService();
        var invoked = false;

        sut.InvokeOnUI(() => invoked = true);

        Assert.True(invoked);
    }

    [Fact]
    public void InvokeOnUI_WhenActionIsNull_ThrowsArgumentNullException()
    {
        var sut = new WpfDispatcherService();

        Assert.Throws<ArgumentNullException>(() => sut.InvokeOnUI(null!));
    }
}
