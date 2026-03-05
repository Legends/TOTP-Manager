using TOTP.Core.Models;
using TOTP.ViewModels;

namespace TOTP.Tests.ViewModels;

public sealed class SettingsExportOptionsControllerTests
{
    [Fact]
    public void Defaults_AreEncryptedTotpWithOpenAfterExportEnabled()
    {
        var sut = new SettingsExportOptionsController();

        Assert.True(sut.ExportEncrypt);
        Assert.Equal(ExportFileFormat.Totp, sut.SelectedExportFormat);
        Assert.True(sut.OpenExportFileAfterExport);
        Assert.True(sut.OpenExportFileAfterExportBeforeEncrypt);
        Assert.False(sut.IsExportFormatSelectionEnabled);
        Assert.False(sut.IsOpenExportFileAfterExportOptionEnabled);
        Assert.Single(sut.AvailableFormats);
        Assert.Equal(ExportFileFormat.Totp, sut.AvailableFormats[0]);
    }

    [Fact]
    public void SetExportEncrypt_SameValue_ReturnsFalseAndDoesNotChangeState()
    {
        var sut = new SettingsExportOptionsController();

        var changed = sut.SetExportEncrypt(true);

        Assert.False(changed);
        Assert.True(sut.ExportEncrypt);
        Assert.Equal(ExportFileFormat.Totp, sut.SelectedExportFormat);
    }

    [Fact]
    public void SetExportEncrypt_False_EnablesPlainFormatsAndSelection()
    {
        var sut = new SettingsExportOptionsController();

        var changed = sut.SetExportEncrypt(false);

        Assert.True(changed);
        Assert.False(sut.ExportEncrypt);
        Assert.True(sut.IsExportFormatSelectionEnabled);
        Assert.True(sut.IsOpenExportFileAfterExportOptionEnabled);
        Assert.Contains(ExportFileFormat.Json, sut.AvailableFormats);
        Assert.Contains(ExportFileFormat.Csv, sut.AvailableFormats);
        Assert.Contains(ExportFileFormat.Txt, sut.AvailableFormats);
        Assert.DoesNotContain(ExportFileFormat.Totp, sut.AvailableFormats);
        Assert.Equal(ExportFileFormat.Json, sut.SelectedExportFormat);
    }

    [Fact]
    public void SetSelectedExportFormat_WhenInvalidForCurrentMode_FallsBackToFirstAvailable()
    {
        var sut = new SettingsExportOptionsController();
        sut.SetExportEncrypt(false);

        var changed = sut.SetSelectedExportFormat(ExportFileFormat.Totp);

        Assert.False(changed);
        Assert.Equal(ExportFileFormat.Json, sut.SelectedExportFormat);
    }

    [Fact]
    public void SetSelectedExportFormat_WhenValid_ChangesSelection()
    {
        var sut = new SettingsExportOptionsController();
        sut.SetExportEncrypt(false);

        var changed = sut.SetSelectedExportFormat(ExportFileFormat.Csv);

        Assert.True(changed);
        Assert.Equal(ExportFileFormat.Csv, sut.SelectedExportFormat);
    }

    [Fact]
    public void SetOpenExportFileAfterExport_WhenEncrypted_IsForcedFalseAndNoChange()
    {
        var sut = new SettingsExportOptionsController();
        sut.SetExportEncrypt(true);

        var changed = sut.SetOpenExportFileAfterExport(true);

        Assert.True(changed);
        Assert.False(sut.OpenExportFileAfterExport);
    }

    [Fact]
    public void SetOpenExportFileAfterExport_WhenPlain_UpdatesCurrentAndMemory()
    {
        var sut = new SettingsExportOptionsController();
        sut.SetExportEncrypt(false);

        var changed = sut.SetOpenExportFileAfterExport(false);

        Assert.True(changed);
        Assert.False(sut.OpenExportFileAfterExport);
        Assert.False(sut.OpenExportFileAfterExportBeforeEncrypt);
    }

    [Fact]
    public void SetExportEncrypt_TrueThenFalse_RestoresRememberedOpenAfterExportValue()
    {
        var sut = new SettingsExportOptionsController();
        sut.SetExportEncrypt(false);
        sut.SetOpenExportFileAfterExport(false);

        sut.SetExportEncrypt(true);

        Assert.False(sut.OpenExportFileAfterExport);
        Assert.False(sut.OpenExportFileAfterExportBeforeEncrypt);

        sut.SetExportEncrypt(false);

        Assert.False(sut.OpenExportFileAfterExport);
        Assert.False(sut.OpenExportFileAfterExportBeforeEncrypt);
    }

    [Fact]
    public void SetOpenExportFileAfterExportBeforeEncrypt_OnlyUpdatesMemoryValue()
    {
        var sut = new SettingsExportOptionsController();
        sut.SetExportEncrypt(true);

        sut.SetOpenExportFileAfterExportBeforeEncrypt(false);

        Assert.False(sut.OpenExportFileAfterExportBeforeEncrypt);
        Assert.True(sut.OpenExportFileAfterExport);

        sut.SetExportEncrypt(false);

        Assert.False(sut.OpenExportFileAfterExport);
    }

    [Theory]
    [InlineData(ExportFileFormat.Json)]
    [InlineData(ExportFileFormat.Csv)]
    [InlineData(ExportFileFormat.Txt)]
    public void SetSelectedExportFormat_WhenPlainMode_AllowsEveryPlainFormat(ExportFileFormat format)
    {
        var sut = new SettingsExportOptionsController();
        sut.SetExportEncrypt(false);

        var changed = sut.SetSelectedExportFormat(format);

        Assert.Equal(format != ExportFileFormat.Json, changed);
        Assert.Equal(format, sut.SelectedExportFormat);
    }

    [Theory]
    [InlineData(ExportFileFormat.Json)]
    [InlineData(ExportFileFormat.Csv)]
    [InlineData(ExportFileFormat.Txt)]
    public void SetSelectedExportFormat_WhenEncryptedMode_AlwaysKeepsTotp(ExportFileFormat requested)
    {
        var sut = new SettingsExportOptionsController();
        sut.SetExportEncrypt(true);

        var changed = sut.SetSelectedExportFormat(requested);

        Assert.False(changed);
        Assert.Equal(ExportFileFormat.Totp, sut.SelectedExportFormat);
        Assert.Equal([ExportFileFormat.Totp], sut.AvailableFormats);
    }

    [Fact]
    public void SetExportEncrypt_TransitionMatrix_PreservesRememberedOpenAfterExportAcrossMultipleToggles()
    {
        var sut = new SettingsExportOptionsController();

        sut.SetExportEncrypt(false);
        sut.SetOpenExportFileAfterExport(true);
        sut.SetExportEncrypt(true);
        Assert.False(sut.OpenExportFileAfterExport);
        Assert.True(sut.OpenExportFileAfterExportBeforeEncrypt);

        sut.SetExportEncrypt(false);
        Assert.True(sut.OpenExportFileAfterExport);

        sut.SetOpenExportFileAfterExport(false);
        sut.SetExportEncrypt(true);
        Assert.False(sut.OpenExportFileAfterExport);
        Assert.False(sut.OpenExportFileAfterExportBeforeEncrypt);

        sut.SetExportEncrypt(false);
        Assert.False(sut.OpenExportFileAfterExport);
    }

    [Fact]
    public void SetExportEncrypt_FalseAfterEncryptedWithTotp_DefaultsFormatToJson()
    {
        var sut = new SettingsExportOptionsController();
        Assert.Equal(ExportFileFormat.Totp, sut.SelectedExportFormat);

        sut.SetExportEncrypt(false);

        Assert.Equal(ExportFileFormat.Json, sut.SelectedExportFormat);
    }
}
