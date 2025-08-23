using System;
using TOTP.Core.Enums;
using TOTP.Resources;

namespace TOTP.Validation;


public static class ValidationMessageMapper
{
    public static string ToMessage(ValidationError error, params string[] args) => error switch
    {
        ValidationError.None => string.Empty,
        ValidationError.PlatformRequired => UI.msg_PlatformRequired,
        ValidationError.SecretRequired => UI.msg_SecretRequired,
        ValidationError.SecretInvalidFormat => UI.msg_SecretInvalidFormat,
        ValidationError.PlatformAlreadyExists => string.Format(UI.msg_Platform_Exists, args),
        _ => throw new MissingMemberException($"Missing ValidationError member")
    };


}
