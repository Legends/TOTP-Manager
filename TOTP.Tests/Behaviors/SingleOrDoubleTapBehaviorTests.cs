using Github2FA.Behaviors;
using Github2FA.Commands;

namespace Github2FA.Tests.Behaviors;

public class SingleOrDoubleTapBehaviorTests
{
    [Fact]
    public void InvokeSingleTap_ShouldExecuteCommand()
    {
        // Arrange
        bool wasCalled = false;
        var behavior = new TestableSingleOrDoubleTapBehavior
        {
            SingleTapCommand = new RelayCommand(() => wasCalled = true)
        };

        // Act
        behavior.InvokeSingleTap();

        // Assert
        Assert.True(wasCalled);
    }

    private class TestableSingleOrDoubleTapBehavior : SingleOrDoubleTapBehavior
    {
        public new void InvokeSingleTap() => base.InvokeSingleTap();
    }
}


