using TOTP.Core.Enums;
using TOTP.Resources;
using TOTP.Validation;

namespace TOTP.Tests.Validation;

public sealed class ValidationMessageMapperTests
{
    [Fact]
    public void ToMessage_MapsKnownValidationErrors()
    {
        Assert.Equal(string.Empty, ValidationMessageMapper.ToMessage(ValidationError.None));
        Assert.Equal(UI.msg_PlatformRequired, ValidationMessageMapper.ToMessage(ValidationError.PlatformRequired));
        Assert.Equal(UI.msg_SecretRequired, ValidationMessageMapper.ToMessage(ValidationError.SecretRequired));
        Assert.Equal(UI.msg_SecretInvalidFormat, ValidationMessageMapper.ToMessage(ValidationError.SecretInvalidFormat));
    }

    [Fact]
    public void ToMessage_PlatformAlreadyExists_FormatsWithArgs()
    {
        var message = ValidationMessageMapper.ToMessage(ValidationError.PlatformAlreadyExists, "GitHub");

        Assert.Equal(string.Format(UI.msg_Platform_Exists, "GitHub"), message);
    }

    [Fact]
    public void ToMessage_UnknownValidationError_ThrowsMissingMemberException()
    {
        var ex = Assert.Throws<MissingMemberException>(() => ValidationMessageMapper.ToMessage((ValidationError)999));

        Assert.Contains("Missing ValidationError member", ex.Message);
    }
}
