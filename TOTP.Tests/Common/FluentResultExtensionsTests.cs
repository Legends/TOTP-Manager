using FluentResults;
using TOTP.Core.Common;

namespace TOTP.Tests.Common;

public sealed class FluentResultExtensionsTests
{
    [Fact]
    public void GetErrorCode_WhenSuccess_ReturnsUnknown()
    {
        var result = Result.Ok();

        Assert.Equal(AppErrorCode.Unknown, result.GetErrorCode());
    }

    [Fact]
    public void GetErrorCode_PrefersAppErrorInstance()
    {
        var result = Result.Fail(new AppError(AppErrorCode.TokensUpdateFailed, "err"));

        Assert.Equal(AppErrorCode.TokensUpdateFailed, result.GetErrorCode());
    }

    [Fact]
    public void GetErrorCode_UsesMetadataFallback()
    {
        var error = new Error("x");
        error.Metadata[AppError.ErrorCodeMetadataKey] = AppErrorCode.ImportInvalidPayload;
        var result = Result.Fail(error);

        Assert.Equal(AppErrorCode.ImportInvalidPayload, result.GetErrorCode());
    }

    [Fact]
    public void GetTechnicalMessage_JoinsErrorsOrEmptyWhenSuccess()
    {
        Assert.Equal(string.Empty, Result.Ok().GetTechnicalMessage());

        var result = Result.Fail("first").WithError("second");
        var message = result.GetTechnicalMessage();

        Assert.Contains("first", message);
        Assert.Contains("second", message);
        Assert.Contains(";", message);
    }
}
