namespace TOTP.Core.Enums;

public enum OperationStatus
{
    Unknown,
    NotFound,
    LoadingFailed,
    DeleteFailed,
    UpdateFailed,
    CreateFailed,
    StorageFailed,
    Success,
    AlreadyExists
}