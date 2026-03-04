using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TOTP.Core.Models;
using TOTP.Commands;
using TOTP.Infrastructure.Extensions;
using TOTP.Resources;

namespace TOTP.ViewModels;

public partial class MainViewModel
{
    #region ### COMMANDS EVENTHANDLER ###

    private void SetupCommandEventhandler()
    {
        CloseSettingsViewCommand = new RelayCommand(
             CloseSettingsView,
             () => IsSettingsViewOpen);

        OpenSettingsCommand = new RelayCommand(OpenSettingsView);

        CopyCodeCommand = new RelayCommand<OtpViewModel>(model => CopyTotpCodeToClipboard());
        GenerateQrCommand = new RelayCommand<OtpViewModel>(model => GenerateQrCodeImage());
        ToggleQrPreviewCommand = new RelayCommand<System.Windows.Media.Imaging.BitmapSource?>(source => _qrPreviewService.Toggle(source));
        ExportSecretsCommand = new AsyncCommand(ExportSecretsToFile);
        ScanQrAndAddCommand = new AsyncCommand(ScanQrAndAddAccountAsync, () => !_isGridInEditMode);

        OpenFlyoutEditModeCommand = new RelayCommand<OtpViewModel>(OpenFlyoutEditMode);
        OpenFlyoutAddModeCommand = new RelayCommand(OpenFlyoutAddMode, () => !_isGridInEditMode);
        SaveEditFlyoutAsyncCommand = new AsyncCommand(AddOrUpdateOtpEntryAsync);
        CancelFlyoutCommand = new RelayCommand(CancelFlyout);

        RowSelectionChangedCommand = new AsyncCommand<OtpViewModel>(OnRowSelectionChangedAsync);
        DeleteSecretCommand = new AsyncCommand<OtpViewModel>(DeleteOtpEntryAsync, null, _logger);
        BeginEditCommand = new RelayCommand<OtpViewModel>(OnBeginEdit);
        EndEditCommand = new AsyncCommand<OtpViewModel>(OnEndEditAsync);
        DoubleClickCommand = new RelayCommand<OtpViewModel>(OnDoubleClick);

        ToggleSearchBoxCommand = new RelayCommand(() =>
        {
            IsSearchVisible = !IsSearchVisible;
            IsSearchFocused = IsSearchVisible;

            if (!IsSearchVisible)
                SearchText = string.Empty;

        }, () => !IsGridEditing);

        ClearSearchCommand = new RelayCommand(ClearSearchTextbox, () => IsSearchVisible);

    }

    private void OpenSettingsView()
    {
        IsSettingsViewOpen = true;
        SettingsVm.RequestFocus();
    }

    private void CloseSettingsView()
    {
        IsSettingsViewOpen = false;
    }

    #endregion COMMANDS SETUP

    private void SaveSettingsView()
    {
        IsSettingsViewOpen = false;
    }

    private async Task ExportOtps(bool toBeEncrypted, ExportFileFormat format)
    {
        try
        {
            var effectiveFormat = toBeEncrypted ? ExportFileFormat.Json : format;
            var defaultExt = GetDefaultExtension(effectiveFormat, toBeEncrypted);
            var filter = GetSaveDialogFilter(effectiveFormat, toBeEncrypted);
            var path = _fileDialogService.ShowSaveFileDialog(
                filter,
                defaultExt,
                "otp-backup"
            );

            if (path == null)
                return;

            var result = await _accountsWorkflow.GetAllEntriesSortedAsync();
            if (result.IsFailed)
            {
                _messageService.ShowResultError(result);
                return;
            }

            FluentResults.Result exportResult;
            if (toBeEncrypted)
            {
                var password = _passwordPromptService.Prompt(UI.ui_Settings_Export, UI.ui_EnterExportPassword);
                if (string.IsNullOrWhiteSpace(password))
                {
                    _messageService.ShowWarning(UI.ui_ExportPasswordRequired);
                    return;
                }

                exportResult = await _exportService.ExportToEncryptedFileAsync(result.Value, password, path, effectiveFormat);
            }
            else
            {
                exportResult = await _exportService.ExportToFileAsync(result.Value, path, effectiveFormat);
            }

            if (exportResult.IsFailed)
            {
                _messageService.ShowResultError(exportResult);
                return;
            }

            var psi = new ProcessStartInfo { FileName = path, UseShellExecute = true };

            Process.Start(psi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Export workflow failed.");
            _messageService.ShowError(UI.ex_UnexpectedError );
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

    private async Task ImportOtps(ImportConflictStrategy strategy)
    {
        try
        {
            const string filter = "TOTP Backups|*.totp;*.json;*.txt;*.csv|Encrypted TOTP Backup|*.totp|JSON Files|*.json|Text Files|*.txt|CSV Files|*.csv";
            var path = _fileDialogService.ShowOpenFileDialog(filter, ".totp");
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            string? password = null;
            var extension = Path.GetExtension(path);
            if (extension.Equals(".totp", StringComparison.OrdinalIgnoreCase))
            {
                password = _passwordPromptService.Prompt(UI.ui_Settings_Import, UI.ui_EnterImportPassword);
                if (string.IsNullOrWhiteSpace(password))
                {
                    _messageService.ShowWarning(UI.ui_ImportPasswordRequired);
                    return;
                }
            }

            var importResult = await _exportService.ImportFromFileAsync(path, password);
            if (importResult.IsFailed)
            {
                _messageService.ShowResultError(importResult);
                return;
            }

            var importedEntries = importResult.Value;
            if (importedEntries.Count == 0)
            {
                _messageService.ShowInfo(UI.ui_Import_NoEntriesFound);
                return;
            }

            var preview = BuildImportPreview(importedEntries, strategy);
            var shouldContinue = _messageService.ConfirmInfo(preview, UI.ui_btnOK, UI.ui_btnCancel);
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
                var existing = FindMatchingEntry(incomingVm);

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
                        var updateResult = await _accountsWorkflow.UpdateAsync(existing, updated);
                        if (updateResult.IsFailed)
                        {
                            failed++;
                            continue;
                        }

                        existing.UpdateSelf(updated);
                        replaced++;
                        continue;
                    }

                    // Keep both
                    incomingVm.ID = Guid.NewGuid();
                    incomingVm.Issuer = CreateKeepBothIssuer(incomingVm.Issuer);
                }

                var addResult = await _accountsWorkflow.AddAsync(incomingVm);
                if (addResult.IsFailed)
                {
                    failed++;
                    continue;
                }

                AllOtps.Add(incomingVm);
                added++;
            }

            OnPropertyChanged(nameof(AllOtps));
            _messageService.ShowInfo(string.Format(UI.ui_Import_ResultSummary, importedEntries.Count, added, replaced, skipped, failed));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Import workflow failed.");
            _messageService.ShowError(UI.ex_UnexpectedError);
        }
    }

    private OtpViewModel? FindMatchingEntry(OtpViewModel incoming)
    {
        var byId = AllOtps.FirstOrDefault(existing => existing.ID == incoming.ID);
        if (byId != null)
        {
            return byId;
        }

        var byIssuerAndAccount = AllOtps.FirstOrDefault(existing =>
            string.Equals(existing.Issuer ?? string.Empty, incoming.Issuer ?? string.Empty, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(existing.AccountName ?? string.Empty, incoming.AccountName ?? string.Empty, StringComparison.OrdinalIgnoreCase));
        if (byIssuerAndAccount != null)
        {
            return byIssuerAndAccount;
        }

        return AllOtps.FirstOrDefault(existing =>
            string.Equals(existing.Issuer ?? string.Empty, incoming.Issuer ?? string.Empty, StringComparison.OrdinalIgnoreCase));
    }

    private string CreateKeepBothIssuer(string? baseIssuer)
    {
        var source = string.IsNullOrWhiteSpace(baseIssuer) ? "Imported" : baseIssuer.Trim();
        var candidate = $"{source} (imported)";
        var suffix = 2;

        while (AllOtps.Any(item => string.Equals(item.Issuer ?? string.Empty, candidate, StringComparison.OrdinalIgnoreCase)))
        {
            candidate = $"{source} (imported {suffix})";
            suffix++;
        }

        return candidate;
    }

    private string BuildImportPreview(System.Collections.Generic.IReadOnlyCollection<OtpEntry> importedEntries, ImportConflictStrategy strategy)
    {
        var conflicts = importedEntries.Count(entry => FindMatchingEntry(entry.ToViewModel()) != null);
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

        return string.Format(
            UI.ui_Import_PreviewSummary,
            importedEntries.Count,
            conflicts,
            GetStrategyDisplayName(strategy),
            projectedAdd,
            projectedReplace,
            projectedSkip);
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
