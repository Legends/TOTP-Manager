using FluentResults;
using System.Collections.Generic;
using System.Threading.Tasks;
using TOTP.Core.Models;

namespace TOTP.Core.Services.Security.Interfaces;

public interface IExportService
{
    /// <summary>
    /// Verschlüsselt die OtpEntries und speichert sie sicher in einer Datei.
    /// </summary>
    Task<Result> ExportToEncryptedFileAsync(IEnumerable<OtpEntry> accounts, string password, string filePath);

    /// <summary>
    /// Liest eine verschlüsselte Datei ein, validiert sie und gibt die Accounts zurück.
    /// </summary>
    Task<Result<List<OtpEntry>>> ImportFromEncryptedFileAsync(string password, string filePath);
}