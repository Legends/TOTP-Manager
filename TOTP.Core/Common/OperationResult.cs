using TOTP.Core.Enums;

namespace TOTP.Core.Common;

public record OperationResult<T>(OperationStatus Status, T Value)
{
    public static OperationResult<T> Success(T value) => new(OperationStatus.Success, value);
    public static OperationResult<T> Fail(OperationStatus status, T value = default) => new(status, value);
}
