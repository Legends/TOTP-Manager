using System.Windows;
using System.Windows.Controls;
using TOTP.Helper;

namespace TOTP.Tests.Services;

public sealed class CommonHelperTests
{
    [UIFact]
    public void PackUriExists_WhenUriInvalid_ReturnsFalse()
    {
        var exists = TOTP.Helper.Common.PackUriExists("pack://application:,,,/does/not/exist.png");

        Assert.False(exists);
    }

    [UIFact]
    public void FindParent_WhenParentExists_ReturnsParent()
    {
        var parent = new Grid();
        var border = new Border();
        var child = new TextBlock();
        border.Child = child;
        parent.Children.Add(border);

        var resolved = TOTP.Helper.Common.FindParent<Border>(child);

        Assert.Same(border, resolved);
    }

    [UIFact]
    public void FindParent_WhenParentDoesNotExist_ReturnsNull()
    {
        var child = new TextBlock();

        var resolved = TOTP.Helper.Common.FindParent<Grid>(child);

        Assert.Null(resolved);
    }
}
