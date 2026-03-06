using FluentResults;
using System.Collections.Generic;
using System.Threading.Tasks;
using TOTP.Core.Models;

namespace TOTP.Core.Services.Interfaces;

public interface IExportService
{
    /// <summary>
    /// Verschlüsselt die OtpEntries und speichert sie sicher in einer Datei.
    /// </summary>
    Task<Result> ExportToEncryptedFileAsync(IEnumerable<Account> accounts, string password, string filePath, ExportFileFormat format);

    /// <summary>
    /// Exportiert die OtpEntries unverschlüsselt im gewünschten Dateiformat.
    /// </summary>
    Task<Result> ExportToFileAsync(IEnumerable<Account> accounts, string filePath, ExportFileFormat format);

    /// <summary>
    /// Liest eine verschlüsselte Datei ein, validiert sie und gibt die Accounts zurück.
    /// </summary>
    Task<Result<List<Account>>> ImportFromEncryptedFileAsync(string password, string filePath);

    /// <summary>
    /// Importiert OTP-Accounts aus einer Datei.
    /// Unterstützt verschlüsselte .totp sowie unverschlüsselte .json/.txt/.csv.
    /// </summary>
    Task<Result<List<Account>>> ImportFromFileAsync(string filePath, string? password = null);
}
