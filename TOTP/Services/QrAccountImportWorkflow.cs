using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TOTP.Core.Enums;
using TOTP.Infrastructure.Parser;
using TOTP.Resources;
using TOTP.Services.Interfaces;
using TOTP.Validation;
using TOTP.ViewModels;

namespace TOTP.Services;

public sealed class QrAccountImportWorkflow(
    IAccountsWorkflowService accountsWorkflow,
    IMessageService messageService,
    ILogger<QrAccountImportWorkflow> logger) : IQrAccountImportWorkflow
{
    public async Task<QrAccountImportResult> ImportAsync(
        string decodedOtpAuthUri,
        ObservableCollection<OtpViewModel> accounts)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(decodedOtpAuthUri);
        ArgumentNullException.ThrowIfNull(accounts);

        OtpauthParser.TOTPData parsed;
        try
        {
            parsed = OtpauthParser.Parse(decodedOtpAuthUri);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to parse scanned OTP URI.");
            messageService.ShowError(UI.msg_ErrorParsingOtpUrl);
            return new QrAccountImportResult(QrAccountImportChangeKind.None);
        }

        var incoming = new OtpViewModel(
            Guid.NewGuid(),
            parsed.Issuer ?? string.Empty,
            parsed.SecretBase32,
            parsed.Label);

        var existing = FindMatchingEntry(incoming, accounts);
        if (existing != null)
        {
            if (SecretsEquivalent(existing.Secret, incoming.Secret))
            {
                messageService.ShowInfo(UI.ui_QrImport_AccountAlreadyExistsNoChanges);
                return new QrAccountImportResult(QrAccountImportChangeKind.None, existing.ID);
            }

            var shouldUpdate = messageService.ConfirmInfo(
                string.Format(UI.ui_QrImport_UpdateExistingPrompt_Format, existing.Issuer),
                UI.ui_QrImport_UpdateExisting,
                UI.ui_QrImport_MoreOptions);

            if (shouldUpdate)
            {
                var updated = incoming.Copy();
                if (updated == null)
                {
                    return new QrAccountImportResult(QrAccountImportChangeKind.None);
                }

                updated.ID = existing.ID;
                var updateResult = await accountsWorkflow.UpdateAsync(existing, updated);
                if (updateResult.IsFailed)
                {
                    messageService.ShowResultError(updateResult, existing.Issuer);
                    return new QrAccountImportResult(QrAccountImportChangeKind.None, existing.ID);
                }

                existing.UpdateSelf(updated);
                messageService.ShowSuccess(UI.ui_QrImport_AccountUpdatedFromQr, 2);
                return new QrAccountImportResult(QrAccountImportChangeKind.Updated, existing.ID);
            }

            var keepBoth = messageService.ConfirmInfo(
                UI.ui_QrImport_KeepBothPrompt,
                UI.ui_Settings_Import_Conflict_KeepBoth,
                UI.ui_btnCancel);

            if (!keepBoth)
            {
                return new QrAccountImportResult(QrAccountImportChangeKind.None);
            }

            incoming.ID = Guid.NewGuid();
            incoming.Issuer = CreateKeepBothIssuer(incoming.Issuer, accounts);
        }

        var validationErrors = accountsWorkflow.ValidateForCreate(incoming, accounts);
        if (validationErrors.Count > 0)
        {
            foreach (var error in validationErrors)
            {
                messageService.ShowError(error == ValidationError.PlatformAlreadyExists
                    ? ValidationMessageMapper.ToMessage(error, incoming.Issuer ?? string.Empty)
                    : ValidationMessageMapper.ToMessage(error));
            }

            return new QrAccountImportResult(QrAccountImportChangeKind.None);
        }

        var addResult = await accountsWorkflow.AddAsync(incoming);
        if (addResult.IsFailed)
        {
            messageService.ShowResultError(addResult, incoming.Issuer);
            return new QrAccountImportResult(QrAccountImportChangeKind.None);
        }

        accounts.Add(incoming);
        return new QrAccountImportResult(QrAccountImportChangeKind.Added, incoming.ID);
    }

    private static OtpViewModel? FindMatchingEntry(
        OtpViewModel incoming,
        IEnumerable<OtpViewModel> accounts)
    {
        var byIssuerAndAccount = accounts.FirstOrDefault(existing =>
            string.Equals(existing.Issuer ?? string.Empty, incoming.Issuer ?? string.Empty, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(existing.AccountName ?? string.Empty, incoming.AccountName ?? string.Empty, StringComparison.OrdinalIgnoreCase));

        return byIssuerAndAccount ?? accounts.FirstOrDefault(existing =>
            string.Equals(existing.Issuer ?? string.Empty, incoming.Issuer ?? string.Empty, StringComparison.OrdinalIgnoreCase));
    }

    private static string CreateKeepBothIssuer(string? baseIssuer, IEnumerable<OtpViewModel> accounts)
    {
        var source = string.IsNullOrWhiteSpace(baseIssuer) ? "Imported" : baseIssuer.Trim();
        var candidate = $"{source} (imported)";
        var suffix = 2;

        while (accounts.Any(item => string.Equals(item.Issuer ?? string.Empty, candidate, StringComparison.OrdinalIgnoreCase)))
        {
            candidate = $"{source} (imported {suffix++})";
        }

        return candidate;
    }

    private static bool SecretsEquivalent(string? left, string? right)
        => string.Equals(NormalizeSecret(left), NormalizeSecret(right), StringComparison.Ordinal);

    private static string NormalizeSecret(string? value)
        => new string((value ?? string.Empty).Where(ch => !char.IsWhiteSpace(ch) && ch != '-').ToArray())
            .TrimEnd('=')
            .ToUpperInvariant();
}
