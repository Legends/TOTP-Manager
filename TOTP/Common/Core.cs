using TOTP.Enums;

namespace TOTP.Core;

public record Result<T>(OperationStatus status, T value)
{
    public static Result<T> Success(T value) => new(OperationStatus.Success, value);
    public static Result<T> Fail(OperationStatus status, T value = default) => new(status, value);
}
