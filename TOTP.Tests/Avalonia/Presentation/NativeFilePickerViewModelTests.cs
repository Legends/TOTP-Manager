using Moq;
using TOTP.Avalonia.Desktop.Platform;
using TOTP.Avalonia.Desktop.Presentation;

namespace TOTP.Tests.Avalonia.Presentation;

public sealed class NativeFilePickerViewModelTests
{
    [Fact]
    public async Task PickAsync_WhenFileSelected_ShowsNameWithoutReadingFile()
    {
        var picker = new Mock<IAvaloniaFilePicker>();
        picker.Setup(value => value.PickImportFileNameAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("backup.totp");
        var sut = new NativeFilePickerViewModel(picker.Object);

        await sut.PickAsync();

        Assert.Contains("backup.totp", sut.Message, StringComparison.Ordinal);
        Assert.Contains("not enabled", sut.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PickAsync_WhenBoundaryThrows_DoesNotExposeExceptionText()
    {
        var picker = new Mock<IAvaloniaFilePicker>();
        picker.Setup(value => value.PickImportFileNameAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("sensitive local path"));
        var sut = new NativeFilePickerViewModel(picker.Object);

        await sut.PickAsync();

        Assert.DoesNotContain("sensitive", sut.Message, StringComparison.OrdinalIgnoreCase);
    }
}
