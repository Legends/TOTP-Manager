using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TOTP.Core.Enums;

namespace TOTP.Core.Common;

using FluentResults;

public class StatusError : Error
{
    public OperationStatus Status { get; }
    public string Step { get; } // e.g., "Backup", "Encryption", "Serialization"

    public StatusError(OperationStatus status, string step = "")
        : base($"Operation {status} during {step}")
    {
        Status = status;
        Step = step;
        // You can also add metadata to the FluentResults dictionary if needed
        //Metadata.Add("ErrorCode", status);
    }

    public static StatusError Create(OperationStatus status)
    {
        return new StatusError(status);
    }
}

