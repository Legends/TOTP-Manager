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
    public void Post_WhenNoApplicationCurrent_ExecutesActionInline()
    {
        var sut = new WpfDispatcherService();
        var invoked = false;

        sut.Post(() => invoked = true);

        Assert.True(invoked);
    }

    [Fact]
    public void Post_WhenActionIsNull_ThrowsArgumentNullException()
    {
        var sut = new WpfDispatcherService();

        Assert.Throws<ArgumentNullException>(() => sut.Post(null!));
    }

    [Fact]
    public async Task InvokeAsync_WithSynchronousAction_ExecutesAction()
    {
        var sut = new WpfDispatcherService();
        var invoked = false;

        await sut.InvokeAsync(() => invoked = true);

        Assert.True(invoked);
    }

    [Fact]
    public async Task InvokeAsync_WithAsynchronousAction_AwaitsCompletion()
    {
        var sut = new WpfDispatcherService();
        var invoked = false;

        await sut.InvokeAsync(async () =>
        {
            await Task.Yield();
            invoked = true;
        });

        Assert.True(invoked);
    }

    [Fact]
    public async Task InvokeAsync_WhenActionIsNull_ThrowsArgumentNullException()
    {
        var sut = new WpfDispatcherService();

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.InvokeAsync((Action)null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.InvokeAsync((Func<Task>)null!));
    }
}
