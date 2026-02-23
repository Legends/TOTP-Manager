using FluentResults;
using System.Linq;
using TOTP.Core.Enums;

namespace TOTP.Core.Common
{
  

    public static class ResultExtensions
    {
        /// <summary>
        /// Extracts the OperationStatus from a Result's error collection.
        /// Falls back to Unknown if no StatusError is found.
        /// </summary>
        public static OperationStatus GetStatus(this IResultBase result)
        {
            if (result.IsSuccess)
                return OperationStatus.Success;

            return result.Errors
                       .OfType<StatusError>()
                       .FirstOrDefault()?.Status
                   ?? OperationStatus.Unknown;
        }
    }
}
