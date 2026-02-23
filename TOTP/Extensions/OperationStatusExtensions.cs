using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TOTP.Core.Enums;
using TOTP.Resources;

namespace TOTP.Extensions;

public static class OperationStatusExtensions
{
    public static string ToLocalizedMessage(this OperationStatus status, string? platform = null, string? fallbackError = null)
    {
        return status switch
        {
            OperationStatus.NotFound => $"{UI.msg_Platform_Not_Found}: {platform}",
            OperationStatus.LoadingFailed => UI.msg_Failed_Loading_Secrets,
            OperationStatus.DeleteFailed => $"{UI.msg_Failed_Delete_Secret}: {platform}",
            OperationStatus.UpdateFailed => $"{UI.msg_Failed_Updating_Secret}: {platform}",
            OperationStatus.CreateFailed => string.Format(UI.msg_FailedAddingSecret, platform ?? ""),
            OperationStatus.StorageFailed => $"{UI.msg_Failed_Storage}: {platform}",
            OperationStatus.AlreadyExists => string.Format(UI.msg_Platform_Exists, platform),
            OperationStatus.Unknown => fallbackError ?? "An unknown error has occurred",
            OperationStatus.Success => string.Empty,
            _ => throw new ArgumentOutOfRangeException(nameof(status))
        };
    }
}