using TOTP.Core.Common;
using TOTP.Resources;
using TOTP.Services;

namespace TOTP.Tests.Services;

public sealed class ResultErrorLocalizerTests
{
    public static IEnumerable<object[]> Mappings =>
    [
        [AppErrorCode.OtpStorageReadFailed, UI.err_OtpStorageReadFailed],
        [AppErrorCode.OtpStorageAccessDenied, UI.err_OtpStorageAccessDenied],
        [AppErrorCode.OtpStorageDecryptFailed, UI.err_OtpStorageDecryptFailed],
        [AppErrorCode.OtpStorageWriteFailed, UI.err_OtpStorageWriteFailed],
        [AppErrorCode.OtpStorageEncryptionFailed, UI.err_OtpStorageEncryptionFailed],
        [AppErrorCode.OtpStorageBackupFailed, UI.err_OtpStorageBackupFailed],
        [AppErrorCode.ExportFileWriteFailed, UI.err_ExportFileWriteFailed],
        [AppErrorCode.ExportFileAccessDenied, UI.err_ExportFileAccessDenied],
        [AppErrorCode.ExportEncryptionFailed, UI.err_ExportEncryptionFailed],
        [AppErrorCode.ExportUnknownFailed, UI.err_ExportUnknownFailed],
        [AppErrorCode.ImportFileNotFound, UI.err_ImportFileNotFound],
        [AppErrorCode.ImportInvalidFile, UI.err_ImportInvalidFile],
        [AppErrorCode.ImportWrongPasswordOrTampered, UI.err_ImportWrongPasswordOrTampered],
        [AppErrorCode.ImportInvalidPayload, UI.err_ImportInvalidPayload],
        [AppErrorCode.ImportUnknownFailed, UI.err_ImportUnknownFailed],
        [AppErrorCode.AccountsLoadFailed, UI.err_TokensLoadFailed],
        [AppErrorCode.AccountsCreateFailed, UI.err_TokensCreateFailed],
        [AppErrorCode.AccountsUpdateFailed, UI.err_TokensUpdateFailed],
        [AppErrorCode.AccountsDeleteFailed, UI.err_TokensDeleteFailed],
        [AppErrorCode.SettingsServiceLoadFailed, UI.err_SettingsServiceLoadFailed],
        [AppErrorCode.SettingsServiceSaveFailed, UI.err_SettingsServiceSaveFailed],
        [AppErrorCode.ClipboardUnavailable, UI.err_ClipboardUnavailable],
        [AppErrorCode.ClipboardWriteFailed, UI.err_ClipboardWriteFailed],
        [AppErrorCode.ClipboardClearFailed, UI.err_ClipboardClearFailed]
    ];

    [Theory]
    [MemberData(nameof(Mappings))]
    public void ToUserMessage_MapsSimpleCodes(AppErrorCode code, string expected)
    {
        var actual = ResultErrorLocalizer.ToUserMessage(code, context: null);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToUserMessage_FormatsContextualCreateUpdateDeleteMessages()
    {
        Assert.Equal(string.Format(UI.err_OtpCreateFailed, "GitHub"), ResultErrorLocalizer.ToUserMessage(AppErrorCode.OtpCreateFailed, "GitHub"));
        Assert.Equal(string.Format(UI.err_OtpUpdateFailed, "GitHub"), ResultErrorLocalizer.ToUserMessage(AppErrorCode.OtpUpdateFailed, "GitHub"));
        Assert.Equal(string.Format(UI.err_OtpDeleteFailed, "GitHub"), ResultErrorLocalizer.ToUserMessage(AppErrorCode.OtpDeleteFailed, "GitHub"));
    }

    [Fact]
    public void ToUserMessage_ContextualMessagesWithoutContext_ReturnTemplateUnformatted()
    {
        Assert.Equal(UI.err_OtpCreateFailed, ResultErrorLocalizer.ToUserMessage(AppErrorCode.OtpCreateFailed, null));
        Assert.Equal(UI.err_OtpUpdateFailed, ResultErrorLocalizer.ToUserMessage(AppErrorCode.OtpUpdateFailed, "   "));
        Assert.Equal(UI.err_OtpDeleteFailed, ResultErrorLocalizer.ToUserMessage(AppErrorCode.OtpDeleteFailed, null));
    }

    [Fact]
    public void ToUserMessage_UnknownCode_ReturnsUnknownMessage()
    {
        var actual = ResultErrorLocalizer.ToUserMessage((AppErrorCode)999, null);

        Assert.Equal(UI.err_Unknown, actual);
    }
}
