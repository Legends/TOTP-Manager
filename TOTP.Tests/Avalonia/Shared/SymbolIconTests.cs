using TOTP.Avalonia.Shared.Controls;

namespace TOTP.Tests.Avalonia.Shared;

public sealed class SymbolIconTests
{
    [Theory]
    [InlineData(SymbolIconKind.Add)]
    [InlineData(SymbolIconKind.Camera)]
    [InlineData(SymbolIconKind.Conceal)]
    [InlineData(SymbolIconKind.Copy)]
    [InlineData(SymbolIconKind.Lock)]
    [InlineData(SymbolIconKind.Reveal)]
    [InlineData(SymbolIconKind.Search)]
    [InlineData(SymbolIconKind.Settings)]
    public void Kind_ProvidesScalableVectorGeometry(SymbolIconKind kind)
    {
        var sut = new SymbolIcon { Kind = kind };

        Assert.StartsWith("M", sut.IconData, StringComparison.Ordinal);
        Assert.Contains("Z", sut.IconData, StringComparison.Ordinal);
    }
}
