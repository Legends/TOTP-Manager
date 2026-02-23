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

        /// <summary>
        /// Gets the primary error message, appended with the "Reason" metadata if it exists.
        /// </summary>
        public static string GetFullMessage(this IResultBase result)
        {
            // 1. Try to find your custom StatusError (which holds the Metadata)
            var error = result.Errors.OfType<StatusError>().FirstOrDefault();

            // 2. If no StatusError found, return the first standard error message
            if (error == null)
                return result.Errors.FirstOrDefault()?.Message ?? "An unknown error occurred.";

            // 3. Try to extract the "Reason" you added in your DAL/Domain
            if (error.Metadata.TryGetValue("Reason", out var reason))
            {
                // Returns: "Storage failed (Path: C:\secrets.json does not exist)"
                return $"{error.Message} ({reason})";
            }

            // 4. Fallback: Just return the standard message
            return error.Message;
        }
    }
}
