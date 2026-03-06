using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TOTP.Core.Common;
using TOTP.Core.Enums;
using TOTP.Core.Models;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Services.Interfaces;
using TOTP.Infrastructure.Extensions;
using TOTP.Resources;
using TOTP.Services.Interfaces;
using TOTP.ViewModels;

namespace TOTP.Services;

public sealed class AccountTransferWorkflowService(
    IFileDialogService fileDialogService,
    IAccountsWorkflowService accountsWorkflow,
    IExportService exportService,
    IPasswordPromptService passwordPromptService,
    IMessageService messageService,
    ISettingsService settingsService,
    ILogger<AccountTransferWorkflowService> logger) : IAccountTransferWorkflowService
{
    public async Task ExportSecretsToFileAsync()
    {
        try
        {
            var path = fileDialogService.ShowSaveFileDialog(".txt|.json", ".json", "Totp-Accounts");

            if (path == null)
            {
                return;
            }

            var result = await accountsWorkflow.GetAllEntriesSortedAsync();
            if (result.IsFailed)
            {
                messageService.ShowResultError(result);
                return;
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(result.Value, options));

            var psi = new ProcessStartInfo { FileName = path, UseShellExecute = true };
            Process.Start(psi);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Export secrets to file failed.");
            messageService.ShowError(UI.ex_UnexpectedError);
        }
    }

    public async Task ExportOtpsAsync(bool toBeEncrypted, ExportFileFormat format)
    {
        try
        {
            var effectiveFormat = toBeEncrypted ? ExportFileFormat.Json : format;
            var defaultExt = GetDefaultExtension(effectiveFormat, toBeEncrypted);
            var filter = GetSaveDialogFilter(effectiveFormat, toBeEncrypted);
            var path = fileDialogService.ShowSaveFileDialog(filter, defaultExt, "otp-backup");

            if (path == null)
            {
                return;
            }

            var result = await accountsWorkflow.GetAllEntriesSortedAsync();
            if (result.IsFailed)
            {
                messageService.ShowResultError(result);
                return;
            }

            FluentResults.Result exportResult;
            if (toBeEncrypted)
            {
                var password = passwordPromptService.PromptForEncryptedExportPassword(UI.ui_Settings_Export);
                if (string.IsNullOrWhiteSpace(password))
                {
                    messageService.ShowWarning(UI.ui_ExportPasswordRequired);
                    return;
                }

                exportResult = await exportService.ExportToEncryptedFileAsync(result.Value, password, path, effectiveFormat);
            }
            else
            {
                exportResult = await exportService.ExportToFileAsync(result.Value, path, effectiveFormat);
            }

            if (exportResult.IsSuccess)
            {
                messageService.ShowSuccess(UI.ui_Settings_Export_Success, 1);
            }

            if (exportResult.IsFailed)
            {
                messageService.ShowResultError(exportResult);
                return;
            }

            if (!toBeEncrypted && settingsService.Current.OpenExportFileAfterExport)
            {
                var psi = new ProcessStartInfo { FileName = path, UseShellExecute = true };
                Process.Start(psi);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Export workflow failed.");
            messageService.ShowError(UI.ex_UnexpectedError);
        }
    }

    public async Task ImportOtpsAsync(ImportConflictStrategy strategy, ObservableCollection<OtpViewModel> allOtps)
    {
        try
        {
            const string filter = "TOTP Backups|*.totp;*.json;*.txt;*.csv|Encrypted TOTP Backup|*.totp|JSON Files|*.json|Text Files|*.txt|CSV Files|*.csv";
            var path = fileDialogService.ShowOpenFileDialog(filter, ".totp");
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            var extension = Path.GetExtension(path);
            FluentResults.Result<List<Account>> importResult;
            if (extension.Equals(".totp", StringComparison.OrdinalIgnoreCase))
            {
                var password = passwordPromptService.Prompt(
                    UI.ui_Settings_Import,
                    UI.ui_EnterImportPassword,
                    requiredErrorMessage: UI.ui_ImportPasswordRequired,
                    validatePasswordAsync: async enteredPassword =>
                    {
                        var validationResult = await exportService.ImportFromFileAsync(path, enteredPassword);
                        return validationResult.GetErrorCode() == AppErrorCode.ImportWrongPasswordOrTampered
                            ? UI.err_ImportWrongPasswordOrTampered
                            : null;
                    });

                if (string.IsNullOrWhiteSpace(password))
                {
                    return;
                }

                importResult = await exportService.ImportFromFileAsync(path, password);
            }
            else
            {
                importResult = await exportService.ImportFromFileAsync(path, null);
            }

            if (importResult.IsFailed)
            {
                messageService.ShowResultError(importResult);
                return;
            }

            var importedEntries = importResult.Value;
            if (importedEntries.Count == 0)
            {
                messageService.ShowInfo(UI.ui_Import_NoEntriesFound);
                return;
            }

            var preview = BuildImportPreview(importedEntries, strategy, allOtps);
            var shouldContinue = messageService.ConfirmInfo(preview, UI.ui_btnOK, UI.ui_btnCancel);
            if (!shouldContinue)
            {
                return;
            }

            var added = 0;
            var replaced = 0;
            var skipped = 0;
            var failed = 0;

            foreach (var incoming in importedEntries)
            {
                var incomingVm = incoming.ToViewModel();
                var existing = FindMatchingEntry(incomingVm, allOtps);

                if (existing != null)
                {
                    if (strategy == ImportConflictStrategy.SkipExisting)
                    {
                        skipped++;
                        continue;
                    }

                    if (strategy == ImportConflictStrategy.ReplaceExisting)
                    {
                        var updated = incoming.ToViewModel();
                        updated.ID = existing.ID;
                        var updateResult = await accountsWorkflow.UpdateAsync(existing, updated);
                        if (updateResult.IsFailed)
                        {
                            failed++;
                            continue;
                        }

                        existing.UpdateSelf(updated);
                        replaced++;
                        continue;
                    }

                    incomingVm.ID = Guid.NewGuid();
                    incomingVm.Issuer = CreateKeepBothIssuer(incomingVm.Issuer, allOtps);
                }

                var addResult = await accountsWorkflow.AddAsync(incomingVm);
                if (addResult.IsFailed)
                {
                    failed++;
                    continue;
                }

                allOtps.Add(incomingVm);
                added++;
            }

            messageService.ShowInfo(string.Format(UI.ui_Import_ResultSummary, importedEntries.Count, added, replaced, skipped, failed).Replace("\\n", Environment.NewLine));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Import workflow failed.");
            messageService.ShowError(UI.ex_UnexpectedError);
        }
    }

    private static string GetDefaultExtension(ExportFileFormat format, bool isEncrypted)
    {
        if (isEncrypted)
        {
            return ".totp";
        }

        return format switch
        {
            ExportFileFormat.Json => ".json",
            ExportFileFormat.Txt => ".txt",
            ExportFileFormat.Csv => ".csv",
            _ => ".json"
        };
    }

    private static string GetSaveDialogFilter(ExportFileFormat format, bool isEncrypted)
    {
        if (isEncrypted)
        {
            return "TOTP Encrypted Backup|*.totp";
        }

        return format switch
        {
            ExportFileFormat.Json => "JSON Files|*.json",
            ExportFileFormat.Txt => "Text Files|*.txt",
            ExportFileFormat.Csv => "CSV Files|*.csv",
            _ => "JSON Files|*.json"
        };
    }

    private static OtpViewModel? FindMatchingEntry(OtpViewModel incoming, ObservableCollection<OtpViewModel> allOtps)
    {
        var byId = allOtps.FirstOrDefault(existing => existing.ID == incoming.ID);
        if (byId != null)
        {
            return byId;
        }

        var byIssuerAndAccount = allOtps.FirstOrDefault(existing =>
            string.Equals(existing.Issuer ?? string.Empty, incoming.Issuer ?? string.Empty, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(existing.AccountName ?? string.Empty, incoming.AccountName ?? string.Empty, StringComparison.OrdinalIgnoreCase));
        if (byIssuerAndAccount != null)
        {
            return byIssuerAndAccount;
        }

        return allOtps.FirstOrDefault(existing =>
            string.Equals(existing.Issuer ?? string.Empty, incoming.Issuer ?? string.Empty, StringComparison.OrdinalIgnoreCase));
    }

    private static string CreateKeepBothIssuer(string? baseIssuer, ObservableCollection<OtpViewModel> allOtps)
    {
        var source = string.IsNullOrWhiteSpace(baseIssuer) ? "Imported" : baseIssuer.Trim();
        var candidate = $"{source} (imported)";
        var suffix = 2;

        while (allOtps.Any(item => string.Equals(item.Issuer ?? string.Empty, candidate, StringComparison.OrdinalIgnoreCase)))
        {
            candidate = $"{source} (imported {suffix})";
            suffix++;
        }

        return candidate;
    }

    private static string BuildImportPreview(IReadOnlyCollection<Account> importedEntries, ImportConflictStrategy strategy, ObservableCollection<OtpViewModel> allOtps)
    {
        var conflicts = importedEntries.Count(entry => FindMatchingEntry(entry.ToViewModel(), allOtps) != null);
        var incoming = importedEntries.Count - conflicts;

        var projectedAdd = strategy switch
        {
            ImportConflictStrategy.SkipExisting => incoming,
            ImportConflictStrategy.ReplaceExisting => incoming,
            ImportConflictStrategy.KeepBoth => importedEntries.Count,
            _ => incoming
        };

        var projectedReplace = strategy == ImportConflictStrategy.ReplaceExisting ? conflicts : 0;
        var projectedSkip = strategy == ImportConflictStrategy.SkipExisting ? conflicts : 0;

        var preview = string.Format(
            UI.ui_Import_PreviewSummary,
            importedEntries.Count,
            conflicts,
            GetStrategyDisplayName(strategy),
            projectedAdd,
            projectedReplace,
            projectedSkip);

        return preview.Replace("\\n", Environment.NewLine);
    }

    private static string GetStrategyDisplayName(ImportConflictStrategy strategy)
    {
        return strategy switch
        {
            ImportConflictStrategy.SkipExisting => UI.ui_Settings_Import_Conflict_Skip,
            ImportConflictStrategy.ReplaceExisting => UI.ui_Settings_Import_Conflict_Replace,
            ImportConflictStrategy.KeepBoth => UI.ui_Settings_Import_Conflict_KeepBoth,
            _ => UI.ui_Settings_Import_Conflict_Skip
        };
    }
}
