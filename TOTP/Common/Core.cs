using TOTP.Enums;

namespace TOTP.Core;

public record OperationResult<T>(OperationStatus status, T value);
